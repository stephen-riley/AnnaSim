using AnnaSim.Cpu;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class BeqInstruction
{
    protected override uint ExecuteImpl(Instruction instruction) => ExecuteBranchOp(Cpu, instruction);

    static internal uint ExecuteBranchOp(AnnaMachine cpu, Instruction instruction)
    {
        var condition = instruction.Idef.Mnemonic switch
        {
            "beq" => cpu.Registers[instruction.Rd] == 0,
            "bne" => cpu.Registers[instruction.Rd] != 0,
            "bgt" => (SignedWord)cpu.Registers[instruction.Rd] > 0,
            "bge" => (SignedWord)cpu.Registers[instruction.Rd] >= 0,
            "blt" => (SignedWord)cpu.Registers[instruction.Rd] < 0,
            "ble" => (SignedWord)cpu.Registers[instruction.Rd] <= 0,
            _ => false
        };

        if (condition)
        {
            return instruction.Idef.NormalizePc((int)cpu.Pc + 1 + instruction.Imm8);
        }
        else
        {
            return instruction.Idef.NormalizePc(cpu.Pc + 1);
        }
    }
}

