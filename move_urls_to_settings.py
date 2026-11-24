#!/usr/bin/env python3
"""
Moves every hard-coded Lipila URL out of the C# code and into appsettings.json.

Run it from the folder that holds MiniLipila.csproj:
    python3 move_urls_to_settings.py            (makes the changes)
    python3 move_urls_to_settings.py --dry-run  (only checks, changes nothing)

It works in two phases:
  1. It prepares every change in memory and checks that each piece of text it
     expects is really there, exactly once.
  2. Only if ALL checks pass does it write anything to disk.
So it either changes everything or changes nothing.
"""
import os
import re
import sys

DRY_RUN = "--dry-run" in sys.argv


def fail(msg):
    print(f"\nSTOPPED, nothing was changed.\n{msg}")
    sys.exit(1)


if not os.path.exists("MiniLipila.csproj"):
    fail("Run this from the folder that holds MiniLipila.csproj.")


def find_file(name):
    hits = []
    for root, dirs, files in os.walk("."):
        dirs[:] = [d for d in dirs if d not in ("bin", "obj") and not d.startswith(".")]
        if name in files:
            hits.append(os.path.join(root, name))
    if len(hits) != 1:
        fail(f"Expected exactly one {name}, found {len(hits)}.")
    return hits[0]


def read(path):
    with open(path, newline="") as f:
        text = f.read()
    crlf = "\r\n" in text
    return text.replace("\r\n", "\n"), crlf


def replace_once(text, old, new, label):
    count = text.count(old)
    if count != 1:
        fail(f"{label}: expected to find this exactly once, found {count} times:\n  {old}")
    return text.replace(old, new)


changes = {}  # path -> (new_text, crlf)

# ---------------------------------------------------------------- appsettings.json
path = find_file("appsettings.json")
text, crlf = read(path)
if '"BaseUrl"' in text:
    fail("appsettings.json already has a Lipila BaseUrl. Has this script already been run?")
text = replace_once(
    text,
    '"AllowedHosts": "*"',
    '"AllowedHosts": "*",\n'
    '  "Lipila": {\n'
    '    "BaseUrl": "https://api.lipila.dev",\n'
    '    "CollectionPath": "/api/v1/collections/mobile-money",\n'
    '    "DisbursementPath": "/api/v1/disbursements/mobile-money",\n'
    '    "CollectionStatusPath": "/api/v1/collections/check-status",\n'
    '    "DisbursementStatusPath": "/api/v1/disbursements/check-status",\n'
    '    "CallbackUrl": "https://lipila.io/callback"\n'
    '  }',
    "appsettings.json",
)
changes[path] = (text, crlf)

# ------------------------------------------------- appsettings.Development.json
path = find_file("appsettings.Development.json")
text, crlf = read(path)
new_text, n = re.subn(r',\s*"Lipila"\s*:\s*\{[^}]*\}', "", text)
if n != 1:
    fail("appsettings.Development.json: could not find the Lipila section to remove.")
changes[path] = (new_text, crlf)

# ------------------------------------------------------------ LipilaSettings.cs
lipila_dir = os.path.dirname(find_file("LipilaPaymentResponse.cs"))
settings_path = os.path.join(lipila_dir, "LipilaSettings.cs")
if os.path.exists(settings_path):
    fail("LipilaSettings.cs already exists. Has this script already been run?")
changes[settings_path] = (
    "namespace MiniLipila.Models.Lipila;\n"
    "\n"
    "public class LipilaSettings\n"
    "{\n"
    "    public string BaseUrl { get; set; } = string.Empty;\n"
    "    public string CollectionPath { get; set; } = string.Empty;\n"
    "    public string DisbursementPath { get; set; } = string.Empty;\n"
    "    public string CollectionStatusPath { get; set; } = string.Empty;\n"
    "    public string DisbursementStatusPath { get; set; } = string.Empty;\n"
    "    public string CallbackUrl { get; set; } = string.Empty;\n"
    "}\n",
    False,
)

# ---------------------------------------------------------------------- Program.cs
path = find_file("Program.cs")
text, crlf = read(path)
text = replace_once(
    text,
    "using MiniLipila.Data;\n",
    "using MiniLipila.Data;\nusing MiniLipila.Models.Lipila;\n",
    "Program.cs (using)",
)
text = replace_once(
    text,
    "var app = builder.Build();",
    '// Read the "Lipila" section of appsettings into LipilaSettings\n'
    'builder.Services.Configure<LipilaSettings>(builder.Configuration.GetSection("Lipila"));\n'
    "\n"
    "var app = builder.Build();",
    "Program.cs (Configure)",
)
changes[path] = (text, crlf)

# ------------------------------------------------------------------- the endpoints
OLD_CTOR = "(AppDbContext db, IConfiguration config)"
NEW_CTOR = "(AppDbContext db, IConfiguration config, IOptions<LipilaSettings> lipila)"
OLD_USING = "using Microsoft.EntityFrameworkCore;\n"
NEW_USING = "using Microsoft.EntityFrameworkCore;\nusing Microsoft.Extensions.Options;\n"

endpoints = {
    "CheckCollectionEndpoint.cs": [
        ('"https://api.lipila.dev/api/v1/collections/check-status"',
         '$"{lipila.Value.BaseUrl}{lipila.Value.CollectionStatusPath}"'),
    ],
    "CheckDisbursementEndpoint.cs": [
        ('"https://api.lipila.dev/api/v1/disbursements/check-status"',
         '$"{lipila.Value.BaseUrl}{lipila.Value.DisbursementStatusPath}"'),
    ],
    "InitiateCollectionEndpoint.cs": [
        ('"https://api.lipila.dev/api/v1/collections/mobile-money"',
         '$"{lipila.Value.BaseUrl}{lipila.Value.CollectionPath}"'),
    ],
    "InitiateDisbursementEndpoint.cs": [
        ('"https://api.lipila.dev/api/v1/disbursements/mobile-money"',
         '$"{lipila.Value.BaseUrl}{lipila.Value.DisbursementPath}"'),
        ('"https://lipila.io/callback"',
         "lipila.Value.CallbackUrl"),
    ],
}

for name, swaps in endpoints.items():
    path = find_file(name)
    text, crlf = read(path)
    text = replace_once(text, OLD_USING, NEW_USING, f"{name} (using)")
    text = replace_once(text, OLD_CTOR, NEW_CTOR, f"{name} (constructor)")
    for old, new in swaps:
        text = replace_once(text, old, new, name)
    changes[path] = (text, crlf)

# ---------------------------------------------------------------- write (phase 2)
print("All checks passed. Files to change:")
for p in changes:
    print("  ", p)

if DRY_RUN:
    print("\nDry run only, nothing was written.")
    sys.exit(0)

for p, (text, crlf) in changes.items():
    with open(p, "w", newline="") as f:
        f.write(text.replace("\n", "\r\n") if crlf else text)

# ---------------------------------------------------------------- proof for the supervisor
print("\nSearching all .cs files for any remaining https:// ...")
left = []
for root, dirs, files in os.walk("."):
    dirs[:] = [d for d in dirs if d not in ("bin", "obj") and not d.startswith(".")]
    for fn in files:
        if fn.endswith(".cs"):
            fp = os.path.join(root, fn)
            with open(fp) as f:
                for i, line in enumerate(f, 1):
                    if "https://" in line or "http://" in line:
                        left.append(f"  {fp}:{i}: {line.strip()}")
if left:
    print("Still found:")
    print("\n".join(left))
else:
    print("None. No web address is left in the code.")

print("\nDone. Next: dotnet build")