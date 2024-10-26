namespace AnnaSim.Extensions;

public static class RangeExtensions
{
    public static bool Contains(this Range self, int n)
        => n >= self.Start.Value && n <= self.End.Value;
}