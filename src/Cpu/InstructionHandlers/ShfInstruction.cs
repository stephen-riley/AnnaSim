using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class ShfInstruction
{
    protected override uint ExecuteImpl(Instruction instruction)
    {
        SignedWord rvalue = Registers[instruction.Rs1];
        SignedWord immvalue = (SignedWord)instruction.Imm6;
        SignedWord result = immvalue > 0 ? rvalue << immvalue : rvalue >> (-immvalue);
        Registers[instruction.Rd] = result;
        return NormalizePc(Pc + 1);
    }
}

