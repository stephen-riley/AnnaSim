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

    public InstructionDefinition() { }

    public string FormatString { get; internal set; } = string.Empty;

    public void ValidateOperands(params Operand[] operands)
    {
        var fillOkay = OperandCount == -1 && operands.Length > 1;
        var mismatch = operands.Length - 1 != OperandCount;

        if (!fillOkay && mismatch)
        {
            throw new InvalidOpcodeException($"{OperandCount} operands required for {Mnemonic}, got {string.Join(' ', (IEnumerable<Operand>)operands)}");
        }
    }

    public bool IsBranch => Mnemonic.StartsWith('b');

    public override string ToString() => $"{Mnemonic}({Type} {Opcode})";
}