namespace AnnaSim.Instructions.Definitions;

public partial class FrameDirective : InstructionDefinition
{
    public FrameDirective() : base()
    {
        Opcode = 0xff;
        Mnemonic = ".frame";
        OperandCount = 1;
        Type = InstructionType.Directive;
        MathOp = MathOperation.NA;
    }
}
