using AnnaSim.Exceptions;

namespace AnnaSim.Instructions.Definitions;

public partial class InInstruction
{
    protected override uint ExecuteImpl(Instruction instruction)
    {
        if (Cpu.Inputs.TryDequeue(out var result))
        {
            Registers[instruction.Rd] = result;
        }
        else
        {
            throw new NoInputRemainingException();
        }
        return NormalizePc(Pc + 1);
    }
}

