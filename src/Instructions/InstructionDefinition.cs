using AnnaSim.Assembler;
using AnnaSim.Exceptions;

namespace AnnaSim.Instructions;

public abstract partial class InstructionDefinition
{
    public const uint DirectiveOpcode = 0xff;

    public uint Opcode { get; set; }
    public string Mnemonic { get; protected set; } = string.Empty;
    public MathOperation MathOp { get; set; } = MathOperation.NA;
    public InstructionType Type { get; set; }
    public int OperandCount { get; set; }
    public string FormatString { get; internal set; } = string.Empty;
    public bool ToStringUnsigned { get; internal set; } = false;

    public InstructionDefinition() { }

    public void ValidateOperands(Operand[] operands)
    {
        var fillOkay = OperandCount == -1 && operands.Length >= 1;
        var mismatch = operands.Length != OperandCount;

        // lw and sw can have 2 operands and assume the third is 0
        if (Mnemonic.EndsWith('w'))
        {
            mismatch = OperandCount is > 3 or < 2;
        }

        if (!fillOkay && mismatch)
        {
            var operandsTerm = operands.Length > 0 ? string.Join(' ', (IEnumerable<Operand>)operands) : "none";
            var requiredCountTerm = OperandCount != -1 ? OperandCount.ToString() : "Some";
            throw new InvalidOpcodeException($"{requiredCountTerm} operands required for {Mnemonic}, got {operandsTerm}");
        }
    }

    public bool IsBranch => Mnemonic.StartsWith('b');

    public override string ToString() => $"{Mnemonic}({Type} {Opcode})";
}