
using System.Numerics;

namespace AnnaSim.Cpu.Memory;
public readonly struct Word : IBitwiseOperators<Word, Word, Word>
{
    internal readonly ushort bits;

    public static readonly Word Zero = new();

    public Word(ushort w) => bits = w;
    public Word(uint w) => bits = (ushort)w;

    public static Word operator ~(Word value) => (uint)~value.bits;

    public static Word operator &(Word left, Word right) => (uint)(left.bits & right.bits);

    public static Word operator |(Word left, Word right) => (uint)(left.bits | right.bits);

    public static Word operator ^(Word left, Word right) => (uint)(left.bits ^ right.bits);

    public static implicit operator ushort(Word w) => (ushort)(w.bits & 0xffff);

    public static implicit operator uint(Word w) => w.bits;

    public static implicit operator Word(uint v) => new(v);

    public static implicit operator Word(MachineWord v) => new(v.bits);

    public Word SetLower(int lower) => (this & 0xff00) | ((uint)lower & 0x00ff);
    public Word SetLower(uint lower) => (this & 0xff00) | (lower & 0x00ff);
    public Word SetUpper(int upper) => (this & 0x00ff) | (uint)(upper & 0xff00);
    public Word SetUpper(uint upper) => (this & 0x00ff) | (upper & 0xff00);

    public string ToString(string fmt) => bits.ToString(fmt);

    public override string ToString() => "0x" + bits.ToString("x4");
}