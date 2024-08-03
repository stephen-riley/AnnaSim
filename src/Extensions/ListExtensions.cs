namespace AnnaSim.Extensions;

public static class ListExtensions
{
    public static List<T> Fluid<T>(this List<T> list, Action<List<T>> action)
    {
        action(list);
        return list;
    }
}