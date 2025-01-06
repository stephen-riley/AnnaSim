using System.Text;
using System.Text.RegularExpressions;
using AnnaSim.Extensions;

namespace AnnaSim.TinyC.Extensions;

public static class StringExtensions
{
    private const string openPattern = @" (\([a-z_]+)";
    private const string closePattern = @"(?<! )\)";

    public static string ToPrettyParseTree(this string s)
    {
        var level = 0;
        s = Regex.Replace(s, openPattern, "\n$1", RegexOptions.NonBacktracking);
        var lines = s.Split("\n");
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            var newLine = new string(line);

            do
            {
                var match = Regex.Match(newLine, closePattern);
                if (match.Success)
                {
                    Emit(sb, newLine[0..match.Index].Trim() + "\n", level);
                    level--;
                    Emit(sb, ")\n", level);
                    newLine = newLine[(match.Index + 1)..].Trim();
                }
                else
                {
                    Emit(sb, newLine.Trim() + "\n", level);
                    newLine = "";
                    level++;
                }
            } while (newLine.Length > 0);
        }
        return sb.ToString();
    }

    private static void Emit(StringBuilder sb, string s, int level)
    {
        var lsb = new StringBuilder();
        lsb.Insert(0, "  ", level < 0 ? 0 : level);
        lsb.Append(s);
        Console.Write(lsb.ToString());
        sb.Append(lsb);
    }

    private static int CountCloseLevels(ref string s)
    {
        var matches = Regex.Matches(s, closePattern);
        int count = matches?.Count ?? 0;
        s = Regex.Replace(s, closePattern, "");
        return -count;
    }
}