namespace AnnaSim.Instructions.Definitions;

public partial class RaliasDirective : InstructionDefinition
{
    public RaliasDirective() : base()
    {
        Opcode = 0xff;
        Mnemonic = ".ralias";
        OperandCount = 2;
        Type = InstructionType.Directive;
        MathOp = MathOperation.NA;
    }
}

