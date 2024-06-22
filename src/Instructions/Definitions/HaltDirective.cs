namespace AnnaSim.Instructions.Definitions;

public partial class HaltDirective : InstructionDefinition
{
    public HaltDirective() : base()
    {
        Opcode = -1;
        Mnemonic = ".halt";
        OperandCount = 0;
        Type = InstructionType.Directive;
        MathOp = MathOperation.NA;
    }
}

