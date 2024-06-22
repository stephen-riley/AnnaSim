namespace AnnaSim.Extensions;

public static class IntegerExtensions
{
    public static int SignExtend(this int n, int bitLength)
    {
        // Push bitLength bits up to the top, then shift down to extend the sign
        int sh = (sizeof(int) * 8) - bitLength;
        int x = n << sh;
        int value = x >> sh;
        return value;
    }

    public static int SignExtend(this short n, int bitLength) => n.SignExtend(bitLength);
}