using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class LwInstruction
{
    protected override uint ExecuteImpl(Instruction instruction)
    {
        int addr = Registers[instruction.Rs1];
        SignedWord immvalue = (SignedWord)instruction.Imm6;
        addr += immvalue;
        Registers[instruction.Rd] = Memory[(uint)addr];
        return NormalizePc(Pc + 1);
    }
}

