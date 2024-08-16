namespace AnnaSim.Extensions;

public static class IEnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> e, Action<T> action)
    {
        foreach (var el in e)
        {
            action(el);
        }
    }

    public static IEnumerable<(int index, T element)> SelectWithIndex<T>(this IEnumerable<T> @this, int startIndex, Func<T, T> selector)
    {
        var result = @this.Select(selector);
        var index = startIndex;
        return result.Select(el => (index++, el));
    }
}