using System.Numerics;

public readonly struct SignedWord : IBitwiseOperators<SignedWord, SignedWord, SignedWord>
{
    internal readonly short bits;

    public SignedWord(short w) => bits = w;
    public SignedWord(int w) => bits = (short)w;

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