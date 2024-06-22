using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class SwInstruction
{
    protected override uint ExecuteImpl(Instruction instruction)
    {
        int addr = Registers[instruction.Rs1];
        SignedWord immvalue = (SignedWord)instruction.Imm6;
        addr += immvalue;
        Memory[(uint)addr] = Registers[instruction.Rd];
        return NormalizePc(Pc + 1);
    }
}

