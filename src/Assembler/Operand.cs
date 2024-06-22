using AnnaSim.Extensions;

namespace AnnaSim.Assembler;

public enum OperandType
{
    Unknown = 0,
    UnsignedInt,
    SignedInt,
    Label,
    Register,
}

public class Operand
{
    public uint UnsignedInt { get; set; }
    public int SignedInt { get; set; }
    public string Str { get; set; } = "";
    public OperandType Type { get; set; }

    public Operand(uint n)
    {
        UnsignedInt = n;
        Type = OperandType.UnsignedInt;
    }

    public Operand(int n)
    {
        SignedInt = n;
        Type = OperandType.SignedInt;
    }

    public Operand(string s, OperandType t)
    {
        if (t is OperandType.Label or OperandType.Register)
        {
            Str = s;
            Type = t;
        }
        else
        {
            throw new InvalidOperationException($"cannot invoke string constructor for Operand with type {t}");
        }
    }

    public static implicit operator Operand(int n) => new(n);

    public static implicit operator Operand(uint n) => new(n);

    public static implicit operator ushort(Operand o) => o.Type == OperandType.UnsignedInt ? (ushort)o.UnsignedInt : throw new InvalidCastException();

    public static implicit operator short(Operand o) => o.Type == OperandType.SignedInt ? (short)o.SignedInt : throw new InvalidCastException();

    public static implicit operator uint(Operand o) => o.Type == OperandType.UnsignedInt ? o.UnsignedInt : throw new InvalidCastException();

    public static implicit operator int(Operand o) => o.Type == OperandType.SignedInt ? o.SignedInt : throw new InvalidCastException();

    public static implicit operator string(Operand o) => o.Type == OperandType.Label ? o.Str : throw new InvalidCastException();

    public static Operand Register(string r) => new(r, OperandType.Register);

    public static Operand Label(string l) => new(l, OperandType.Label);

    public static Operand Parse(string s, OperandType type = OperandType.UnsignedInt)
    {
        if (s.StartsWith('&'))
        {
            return Label(s);
        }
        else if (s.StartsWith('r'))
        {
            return Register(s);
        }

        int negative = s.StartsWith('-') ? 1 : 0;
        int radix = s[negative..].StartsWith("0x") ? 16 : 10;
        int value = Convert.ToInt32(s[(negative + 2)..], radix);

        return type == OperandType.SignedInt ? new Operand(value) : new Operand((uint)value);
    }

    public bool IsStandardRegisterName() => Type == OperandType.Label ? Str.IsStandardRegisterName() : throw new InvalidCastException();

    public override string ToString() => Type switch
    {
        OperandType.SignedInt => SignedInt.ToString(),
        OperandType.UnsignedInt => UnsignedInt.ToString(),
        OperandType.Label => Str,
        _ => throw new InvalidOperationException("ToString() on an Operand of type Unknown")
    };
}