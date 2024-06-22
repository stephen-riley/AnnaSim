using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class AddiInstruction
{
    protected override uint ExecuteImpl(Instruction instruction)
    {
        SignedWord rvalue = Registers[instruction.Rs1];
        SignedWord immvalue = (SignedWord)instruction.Imm6;
        SignedWord result = rvalue + immvalue;
        Registers[instruction.Rd] = result;
        return NormalizePc(Pc + 1);
    }
}

