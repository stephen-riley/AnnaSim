
using System.Numerics;

namespace AnnaSim.Cpu.Memory;
public readonly struct SignedWord : IBitwiseOperators<SignedWord, SignedWord, SignedWord>
{
    internal readonly short bits;

    public SignedWord(short w) => bits = w;
    public SignedWord(int w) => bits = (short)w;

    public SignedWord(short w, int bitLength)
    {
        int sh = (sizeof(short) * 8) - bitLength;
        // push everything up to the top, so sign bit is in high bit
        short x = (short)(w << sh);
        // now sign-extend back down
        bits = (short)(x >> sh);
    }

    public SignedWord(int w, int bitLength)
    {
        int sh = (sizeof(short) * 8) - bitLength;
        // push everything up to the top, so sign bit is in high bit
        short x = (short)(w << sh);
        // now sign-extend back down
        bits = (short)(x >> sh);
    }

    public static SignedWord operator ~(SignedWord value) => ~value.bits;

    public static SignedWord operator &(SignedWord left, SignedWord right) => left.bits & right.bits;

    public static SignedWord operator |(SignedWord left, SignedWord right) => left.bits | right.bits;

    public static SignedWord operator ^(SignedWord left, SignedWord right) => left.bits ^ right.bits;

    public static implicit operator short(SignedWord w) => (short)(w.bits & 0xffff);

    public static implicit operator int(SignedWord w) => w.bits;

    public static implicit operator Word(SignedWord w) => new((ushort)w.bits);

    public static implicit operator SignedWord(int v) => new(v);

    public static implicit operator SignedWord(Word w) => new((short)w.bits);

    public static implicit operator SignedWord(MachineWord v) => new((short)v.bits);

    public string ToString(string fmt) => bits.ToString(fmt);

    public override string ToString() => bits.ToString();

    public bool IsBreakpoint => (bits & 0x80000000) != 0;
}