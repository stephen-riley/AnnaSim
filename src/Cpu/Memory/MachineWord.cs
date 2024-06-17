
using System.Numerics;

namespace AnnaSim.Cpu.Memory;
public struct MachineWord : IBitwiseOperators<MachineWord, MachineWord, MachineWord>
{
    internal uint bits;

    public MachineWord(ushort w) => bits = w;
    public MachineWord(uint w) => bits = w;

    public static MachineWord operator ~(MachineWord value) => ~value;

    public static MachineWord operator &(MachineWord left, MachineWord right) => left & right;

    public static MachineWord operator |(MachineWord left, MachineWord right) => left | right;

    public static MachineWord operator ^(MachineWord left, MachineWord right) => left ^ right;

    public static implicit operator uint(MachineWord w) => w.bits;

    public static implicit operator MachineWord(Word v) => new(v.bits);

    public static implicit operator MachineWord(uint v) => new(v);

    public readonly string ToString(string fmt) => bits.ToString(fmt);

    public override readonly string ToString()
    {
        return (bits >> 16).ToString("x4") + " " + bits.ToString("x4");
    }

    public bool IsBreakpoint => (bits & 0x80000000) != 0;

    public void SetBreakpoint() => bits |= 0x80000000;
}