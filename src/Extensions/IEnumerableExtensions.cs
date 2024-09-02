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

    public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action)
    {
        var i = 0;
        foreach (var e in ie) action(e, i++);
    }

    public static void EachReverse<T>(this IEnumerable<T> ie, Action<T, int> action)
    {
        var i = 0;
        foreach (var e in ie.Reverse()) action(e, i++);
    }
}