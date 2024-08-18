namespace AnnaSim.Instructions.Definitions;

public partial class DefDirective : InstructionDefinition
{
    public DefDirective() : base()
    {
        Opcode = 0xff;
        Mnemonic = ".def";
        OperandCount = 1;
        Type = InstructionType.Directive;
        MathOp = MathOperation.NA;
    }
}
