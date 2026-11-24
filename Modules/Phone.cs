namespace MiniLipila.Modules;

public static class Phone
{
    public static string? ToLipilaFormat(string input)
    {
        var digits = new string(input.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("0")) digits = "260" + digits[1..];
        return digits.StartsWith("260") && digits.Length == 12 ? digits : null;
    }
}
