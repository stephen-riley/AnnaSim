namespace AnnaSim.Instructions.Definitions;

public partial class LuiInstruction
{
    protected override uint ExecuteImpl(Instruction instruction)
    {
        var rdvalue = Registers[instruction.Rd];
        rdvalue = (rdvalue & 0x00ff) | ((uint)instruction.Imm8 << 8);
        Registers[instruction.Rd] = rdvalue;
        return NormalizePc(Pc + 1);
    }
}

