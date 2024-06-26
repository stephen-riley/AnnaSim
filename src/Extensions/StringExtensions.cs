namespace AnnaSim.Extensions;

public static class StringExtensions
{
    public static bool IsStandardRegisterName(this string s) => s.Length == 2 && s.StartsWith('r') && Char.IsDigit(s[1]);
}