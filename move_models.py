#!/usr/bin/env python3
"""
Cleans up the endpoint files by moving every request / response / Lipila class
into its own file under Models/, and the Phone helper into Modules/.

Run from the project folder (the one containing Program.cs):

    python3 move_models.py --dry-run   # show the plan, write nothing
    python3 move_models.py             # apply it

What it does
  * one class per file, named after the class
  * namespaces match the folder (MiniLipila.Models.Requests, ...)
  * adds the needed `using` lines to every file that uses a moved class
  * moves PaymentSettler.cs from Models/ back to Modules/ (it is a service)
  * removes `using` lines that became unused in the files it touched
  * NO renames, NO behaviour changes, NO deleting of unused classes

Backups of every changed or removed file go to ./.move_backup/
Safe to run twice: classes that were already moved are skipped.
"""
import re
import shutil
import sys
from pathlib import Path

DRY = "--dry-run" in sys.argv
ROOT = Path(".").resolve()
BACKUP = ROOT / ".move_backup"
SKIP_DIRS = {"bin", "obj", "Migrations", ".git", ".idea", ".lipila_backup", ".move_backup"}
M = re.MULTILINE

# folder -> (namespace, [class names])
PLAN = {
    "Models/Requests": ("MiniLipila.Models.Requests", [
        "CreateCustomerRequest", "CreateMerchantRequest", "CollectionReq",
        "DisbursementReq", "GetMerchantReq", "WalletRequest",
    ]),
    "Models/Responses": ("MiniLipila.Models.Responses", [
        "CustomerResponse", "CreateMerchantResponse", "MerchantResponse",
        "WalletResponse", "CollectionResponse", "DisbursementResponse",
        "CollectionStatusResponse", "DisbursementStatusResponse",
        "MerchantBalanceResponse",
    ]),
    "Models/Lipila": ("MiniLipila.Models.Lipila", [
        "LipilaPaymentResponse", "PaymentRequest", "LipilaDisbursementBody",
    ]),
    "Modules": ("MiniLipila.Modules", ["Phone"]),
}

# database entities: only needed as `using` for the file we relocate (PaymentSettler)
ENTITIES = {n: "MiniLipila.Models" for n in
            ["Merchant", "Wallet", "Payment", "Customer", "BaseEntity", "PaymentStatus", "PaymentType"]}

NS_RE = re.compile(r"^namespace\s+([\w.]+)\s*;", M)

TYPES = {}   # class name -> namespace
DEST = {}    # class name -> destination path
for folder, (ns, names) in PLAN.items():
    for n in names:
        TYPES[n] = ns
        DEST[n] = ROOT / folder / f"{n}.cs"

log, warnings = [], []


# ----------------------------------------------------------------- helpers
def project_files():
    out = []
    for p in ROOT.rglob("*.cs"):
        if set(p.relative_to(ROOT).parts[:-1]) & SKIP_DIRS:
            continue
        out.append(p)
    return sorted(out)


def rel(p):
    return str(p.relative_to(ROOT))


def match_close(t, i):
    """Index just after the '}' that closes the '{' at position i.
    Skips strings, chars and comments so braces inside them are ignored."""
    n, depth = len(t), 0
    while i < n:
        c, two = t[i], t[i:i + 2]
        if two == "//":
            j = t.find("\n", i)
            i = n if j == -1 else j
            continue
        if two == "/*":
            j = t.find("*/", i + 2)
            i = n if j == -1 else j + 2
            continue
        if c == '"':
            if t.startswith('"""', i):
                q = 0
                while i + q < n and t[i + q] == '"':
                    q += 1
                j = t.find('"' * q, i + q)
                i = n if j == -1 else j + q
                continue
            verbatim = (i > 0 and t[i - 1] == "@") or t[max(i - 2, 0):i] == "@$"
            i += 1
            while i < n:
                if verbatim:
                    if t[i] == '"':
                        if t[i + 1:i + 2] == '"':
                            i += 2
                            continue
                        break
                else:
                    if t[i] == "\\":
                        i += 2
                        continue
                    if t[i] == '"':
                        break
                i += 1
            i += 1
            continue
        if c == "'":
            j = t.find("'", i + 3) if t[i + 1:i + 2] == "\\" else i + 2
            i = (n if j == -1 else j) + 1
            continue
        if c == "{":
            depth += 1
        elif c == "}":
            depth -= 1
            if depth == 0:
                return i + 1
        i += 1
    raise ValueError("unbalanced braces")


def class_regex(name):
    return re.compile(
        rf"^[ \t]*(?:public |internal )?(?:static |sealed |abstract )?(?:partial )?class {name}\b", M)


def block_bounds(text, m):
    """Start/end of a class including attached attribute and comment lines."""
    start = m.start()
    while start > 0:
        prev_end = start - 1
        prev_start = text.rfind("\n", 0, prev_end) + 1
        prev = text[prev_start:prev_end].strip()
        if prev.startswith(("//", "[")):
            start = prev_start
        else:
            break
    brace = text.index("{", m.end())
    end = match_close(text, brace)
    return start, end


def tidy(text):
    text = re.sub(r"\n{3,}", "\n\n", text)
    text = re.sub(r"\{\n[ \t]*\n", "{\n", text)
    return text.rstrip() + "\n"


def ns_of(text):
    m = NS_RE.search(text)
    return m.group(1) if m else None


def visible(file_ns, target_ns):
    return file_ns == target_ns or file_ns.startswith(target_ns + ".")


def add_using(text, ns):
    line = f"using {ns};"
    if line in text.split("\n"):
        return text
    lines = text.split("\n")
    last = -1
    for idx, l in enumerate(lines):
        if re.match(r"^using [\w.]+;\s*$", l):
            last = idx
    if last == -1:
        return line + "\n\n" + text
    lines.insert(last + 1, line)
    return "\n".join(lines)


def build_new_file(ns, cls_text):
    usings = []
    if "[Json" in cls_text:
        usings.append("System.Text.Json.Serialization")
    head = "".join(f"using {u};\n" for u in usings)
    if head:
        head += "\n"
    return f"{head}namespace {ns};\n\n{cls_text}"


def body_without_usings(text):
    return re.sub(r"^using .*;\s*$", "", text, flags=M)


# ------------------------------------------------------------------- load
texts, crlf = {}, set()
for p in project_files():
    raw = p.read_bytes().decode("utf-8-sig")
    if "\r\n" in raw:
        crlf.add(p)
    texts[p] = raw.replace("\r\n", "\n")
original = dict(texts)

if not any(p.name == "Program.cs" for p in texts if p.parent == ROOT):
    print("Run this from the folder that contains Program.cs.")
    sys.exit(1)

created = {}      # path -> text (new files)
touched = set()   # existing files we changed
deleted = set()   # files that were moved away
relocated = set()

# ------------------------------------------------- step 1: extract classes
for name, dest in DEST.items():
    if dest.exists() or dest in created:
        log.append(f"  = {name}: already at {rel(dest)}, skipped")
        continue
    hits = [(p, m) for p, t in texts.items() if (m := class_regex(name).search(t))]
    if not hits:
        warnings.append(f"{name}: class not found anywhere - skipped")
        continue
    if len(hits) > 1:
        warnings.append(f"{name}: found in {len(hits)} files ({', '.join(rel(p) for p, _ in hits)}) - skipped")
        continue
    path, m = hits[0]
    text = texts[path]
    try:
        start, end = block_bounds(text, m)
    except ValueError:
        warnings.append(f"{name}: could not read its braces in {rel(path)} - skipped")
        continue
    cls_text = text[start:end].strip("\n") + "\n"
    texts[path] = tidy(text[:start] + text[end:])
    created[dest] = build_new_file(TYPES[name], cls_text)
    touched.add(path)
    log.append(f"  {name:<28} {path.name}  ->  {rel(dest)}")

# ------------------------------------- step 2: PaymentSettler back to Modules
settlers = [p for p in texts if p.name == "PaymentSettler.cs"]
if len(settlers) == 1 and settlers[0].parent.name == "Models":
    src = settlers[0]
    dest = ROOT / "Modules" / "PaymentSettler.cs"
    if dest.exists():
        warnings.append("Modules/PaymentSettler.cs already exists - PaymentSettler not moved")
    else:
        t = texts.pop(src)
        if NS_RE.search(t):
            t = NS_RE.sub("namespace MiniLipila.Modules;", t, 1)
        else:
            warnings.append("PaymentSettler.cs: no file-scoped namespace found - set it by hand")
        texts[dest] = t
        deleted.add(src)
        relocated.add(dest)
        touched.add(dest)
        log.append(f"  {'PaymentSettler (+SettleResult)':<28} {rel(src)}  ->  {rel(dest)}")
elif len(settlers) == 1:
    log.append("  = PaymentSettler.cs already outside Models/, left alone")
else:
    warnings.append("PaymentSettler.cs not found anywhere - restore it from .move_backup/Models/ first")

# ------------------------------------------------ step 3: fix using lines
def fix_usings(path, text, is_relocated):
    ns = ns_of(text)
    if ns is None:
        warnings.append(f"{rel(path)}: no file-scoped namespace - check its usings by hand")
        return text
    body = body_without_usings(text)
    wanted = dict(TYPES)
    if is_relocated:
        wanted.update(ENTITIES)
    needed = set()
    for cls, tns in wanted.items():
        if visible(ns, tns):
            continue
        if re.search(rf"\b{cls}\b", body):
            needed.add(tns)
    for tns in sorted(needed):
        text = add_using(text, tns)
    return text


for path in list(texts):
    new = fix_usings(path, texts[path], path in relocated)
    if new != texts[path]:
        texts[path] = new
        touched.add(path)
for path in list(created):
    created[path] = fix_usings(path, created[path], False)

# remove usings that became unused in files we touched
for path in touched:
    t = texts.get(path)
    if t is None:
        continue
    body = body_without_usings(t)
    ns = ns_of(t) or ""
    if "using MiniLipila.Modules.Endpoints;" in t and not ns.startswith("MiniLipila.Modules.Endpoints") \
            and not re.search(r"\b\w+Endpoint\b", body):
        t = t.replace("using MiniLipila.Modules.Endpoints;\n", "")
    if "using System.Text.Json.Serialization;" in t and "Json" not in body:
        t = t.replace("using System.Text.Json.Serialization;\n", "")
    texts[path] = t

# ---------------------------------------------------------- step 4: checks
all_texts = {**texts, **created}
for name, dest in DEST.items():
    if dest not in all_texts and not dest.exists():
        continue
    defs = [p for p, t in all_texts.items() if class_regex(name).search(t)]
    if len(defs) > 1:
        warnings.append(f"{name}: now defined in {len(defs)} files - check by hand")

for path, t in all_texts.items():
    if t.count("{") != t.count("}"):
        warnings.append(f"{rel(path)}: braces look unbalanced - check by hand")

for path, t in texts.items():
    if "Endpoints" in path.parts:
        extra = [c for c in re.findall(r"^public (?:static |sealed |abstract )?class (\w+)", t, M)
                 if not c.endswith("Endpoint")]
        if extra:
            warnings.append(f"{rel(path)}: still contains {', '.join(extra)}")

unused = []
for name in DEST:
    own = DEST[name]
    others = "\n".join(t for p, t in all_texts.items() if p != own)
    if name in TYPES and own in created and not re.search(rf"\b{name}\b", others):
        unused.append(name)

# ------------------------------------------------------------------ report
print("\nDRY RUN - nothing written\n" if DRY else "")
print("Moves:")
print("\n".join(log) if log else "  (nothing to move)")
print("\nFiles gaining or losing `using` lines / namespaces:")
for p in sorted(touched, key=rel):
    print(f"  ~ {rel(p)}")
if unused:
    print("\nLooks UNUSED (moved as-is, not deleted - ask before removing):")
    for n in unused:
        print(f"  ? {n}")
if warnings:
    print("\nWARNINGS:")
    for w in warnings:
        print(f"  ! {w}")
else:
    print("\nNo warnings.")

if DRY:
    sys.exit(0)


# ------------------------------------------------------------------- write
def write(path, text):
    path.parent.mkdir(parents=True, exist_ok=True)
    data = text.replace("\n", "\r\n") if path in crlf else text
    # open() with newline="" works on every Python 3 (Path.write_text(newline=) is 3.10+)
    with open(path, "w", encoding="utf-8", newline="") as f:
        f.write(data)


def backup(path):
    target = BACKUP / path.relative_to(ROOT)
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(path, target)


changed = [p for p in texts if texts[p] != original.get(p)]

# 1. back everything up
for p in changed:
    if p.exists():
        backup(p)
for p in deleted:
    backup(p)

# 2. write all changed and new files
for p in changed:
    write(p, texts[p])
for p, t in created.items():
    write(p, t)

# 3. only now remove the old files, so a crash above can never lose a file
for p in deleted:
    p.unlink()

print("\nDone. Backups are in .move_backup/ (delete it once you are happy, or keep it out of git).")
print("Next: dotnet build && dotnet run")