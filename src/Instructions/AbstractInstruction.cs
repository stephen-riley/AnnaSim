using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public abstract class AbstractInstruction
{
    public int Opcode { get; set; }
    public string Mnemonic { get; protected set; } = string.Empty;
    public MathOperation MathOp { get; set; } = MathOperation.NA;
    public InstructionType Type { get; set; }
    public int OperandCount { get; set; }

    public AbstractInstruction() { }

    public abstract void Assemble(AnnaAssembler asm);
    public abstract uint Execute(AnnaMachine cpu, params string[] operands);
}