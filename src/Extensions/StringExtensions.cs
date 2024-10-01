namespace AnnaSim.Extensions;

public static class StringExtensions
{
    public static bool IsStandardRegisterName(this string s) => s.Length == 2 && s.StartsWith('r') && char.IsDigit(s[1]);

    public static string ToWidth(this string s, int width)
        => width - s.Length <= 0 ? s : s + new string(' ', width - s.Length);
}