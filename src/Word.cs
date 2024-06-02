using System.Numerics;

public struct Word : IBitwiseOperators<Word, Word, Word>
{
    internal readonly uint word;

    public Word(ushort w) => word = w;
    public Word(uint w) => word = w;

    public static Word operator ~(Word value) => ~value;

    public static Word operator &(Word left, Word right) => left.word & right.word;

    public static Word operator |(Word left, Word right) => left.word | right.word;

    public static Word operator ^(Word left, Word right) => left.word ^ right.word;

    public static implicit operator ushort(Word w) => (ushort)(w.word & 0xffff);

    public static implicit operator uint(Word w) => w.word;

    public static implicit operator Word(uint v) => new(v);

    public static implicit operator Word(MachineWord v) => new(v.machineWord);

    public string ToString(string fmt) => word.ToString(fmt);

    public bool IsBreakpoint => (word & 0x80000000) != 0;
}