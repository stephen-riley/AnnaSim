namespace AnnaSim.Instructions.Definitions;

public partial class OrgDirective : InstructionDefinition
{
    public OrgDirective() : base()
    {
        Opcode = 0xff;
        Mnemonic = ".org";
        OperandCount = 1;
        Type = InstructionType.Directive;
        MathOp = MathOperation.NA;
    }
}
