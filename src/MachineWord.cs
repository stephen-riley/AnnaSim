using System.Numerics;

public struct MachineWord : IBitwiseOperators<MachineWord, MachineWord, MachineWord>
{
    internal uint machineWord;

    public MachineWord(ushort w) => machineWord = w;
    public MachineWord(uint w) => machineWord = w;

    public static MachineWord operator ~(MachineWord value) => ~value;

    public static MachineWord operator &(MachineWord left, MachineWord right) => left & right;

    public static MachineWord operator |(MachineWord left, MachineWord right) => left | right;

    public static MachineWord operator ^(MachineWord left, MachineWord right) => left ^ right;

    public static implicit operator uint(MachineWord w) => w.machineWord;

    public static implicit operator MachineWord(Word v) => new(v.word);

    public static implicit operator MachineWord(uint v) => new(v);

    public string ToString(string fmt) => machineWord.ToString(fmt);

    public bool IsBreakpoint => (machineWord & 0x80000000) != 0;

    public void SetBreakpoint() => machineWord |= 0x80000000;
}