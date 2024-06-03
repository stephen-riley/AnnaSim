namespace AnnaSim.Test;

public static class IntegerExtensions
{
    public static int SignExtend(this int n, int bitLength)
    {
        int sh = (sizeof(int) * 8) - bitLength;
        // push everything up to the top, so sign bit is in high bit
        int x = (n << sh);
        // now sign-extend back down
        int value = (x >> sh);
        return value;
    }

    public static int SignExtend(this short n, int bitLength) => (int)n.SignExtend(bitLength);
}