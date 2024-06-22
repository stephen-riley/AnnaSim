namespace AnnaSim.Instructions.Definitions;

public partial class FillDirective : InstructionDefinition
{
    public FillDirective() : base()
    {
        Opcode = -1;
        Mnemonic = ".fill";
        OperandCount = -1;
        Type = InstructionType.Directive;
        MathOp = MathOperation.NA;
    }
}

