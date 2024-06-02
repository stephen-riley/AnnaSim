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
}