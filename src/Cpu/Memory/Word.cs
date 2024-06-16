
using System.Numerics;

namespace AnnaSim.Cpu.Memory;
public readonly struct Word : IBitwiseOperators<Word, Word, Word>
{
    internal readonly ushort bits;

    public Word(ushort w) => bits = w;
    public Word(uint w) => bits = (ushort)w;

    public static Word operator ~(Word value) => ~value;

    public static Word operator &(Word left, Word right) => (uint)(left.bits & right.bits);

    public static Word operator |(Word left, Word right) => (uint)(left.bits | right.bits);

    public static Word operator ^(Word left, Word right) => (uint)(left.bits ^ right.bits);

    public static implicit operator ushort(Word w) => (ushort)(w.bits & 0xffff);

    public static implicit operator uint(Word w) => w.bits;

    public static implicit operator Word(uint v) => new(v);

    public static implicit operator Word(MachineWord v) => new(v.bits);

    public string ToString(string fmt) => bits.ToString(fmt);

    public override string ToString() => "0x" + bits.ToString("x4");
}