namespace AnnaSim.Instructions.Definitions;

public partial class CstrDirective : InstructionDefinition
{
    public CstrDirective() : base()
    {
        Opcode = 0xff;
        Mnemonic = ".cstr";
        OperandCount = -1;
        Type = InstructionType.Directive;
        MathOp = MathOperation.NA;
    }
}
