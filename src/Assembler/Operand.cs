using AnnaSim.Extensions;

namespace AnnaSim.Assembler;

public enum OperandType
{
    Unknown = 0,
    UnsignedInt,
    SignedInt,
    Label,
    Register,
    String,
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
        else if (t is OperandType.String)
        {
            Str = s;
            Type = t;
        }
        else
        {
            throw new InvalidOperationException($"cannot invoke string constructor for Operand with type {t}");
        }
    }

    public uint AsUInt()
    {
        return Type switch
        {
            OperandType.UnsignedInt => UnsignedInt,
            OperandType.SignedInt => (uint)SignedInt,
            OperandType.Label => 0xffff,
            _ => throw new InvalidCastException($"Operand is of type {Type}, requested (uint) or (int)")
        };
    }

    public int AsInt()
    {
        return Type switch
        {
            OperandType.UnsignedInt => (int)UnsignedInt,
            OperandType.SignedInt => SignedInt,
            OperandType.Label => 0xffff,
            _ => throw new InvalidCastException($"Operand is of type {Type}, requested (uint) or (int)")
        };
    }

    public static implicit operator Operand(int n) => new(n);

    public static implicit operator Operand(uint n) => new(n);

    public static explicit operator ushort(Operand o) => o.Type switch
    {
        OperandType.UnsignedInt => (ushort)o.UnsignedInt,
        OperandType.SignedInt => (ushort)o.SignedInt,
        OperandType.Label => 0xffff,
        _ => throw new InvalidCastException($"Operand is of type {o.Type}, requested (ushort)")
    };

    public static explicit operator short(Operand o) => o.Type switch
    {
        OperandType.UnsignedInt => (short)o.UnsignedInt,
        OperandType.SignedInt => (short)o.SignedInt,
        OperandType.Label => -32768, // 0xffff
        _ => throw new InvalidCastException($"Operand is of type {o.Type}, requested (ushort)")
    };

    public static explicit operator uint(Operand o) => o.Type == OperandType.UnsignedInt ? o.UnsignedInt : throw new InvalidCastException($"Operand is of type {o.Type}, requested (uint)");

    public static explicit operator int(Operand o) => o.Type == OperandType.SignedInt ? o.SignedInt : throw new InvalidCastException($"Operand is of type {o.Type}, requested (int)");

    public static explicit operator string(Operand o) => o.Type is OperandType.Label or OperandType.Register ? o.Str : throw new InvalidCastException($"Operand is of type {o.Type}, requested (string)");

    public static Operand Register(string r) => new(r, OperandType.Register);

    public static Operand Label(string l) => new(l, OperandType.Label);

    public static Operand String(string s) => new(s, OperandType.String);

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
        else if (s.StartsWith('"'))
        {
            return String(s[1..^1]);
        }

        int negative = s.StartsWith('-') ? 1 : 0;
        int radix = s[negative..].StartsWith("0x") ? 16 : s[negative..].StartsWith("0b") ? 2 : 10;
        int value = Convert.ToInt32(s[(negative + (radix != 10 ? 2 : 0))..], radix);
        value = negative == 1 ? -value : value;

        return type == OperandType.SignedInt ? new Operand(value) : new Operand((uint)value);
    }

    public bool IsStandardRegisterName() => Type == OperandType.Register ? Str.IsStandardRegisterName() : throw new InvalidCastException();

    public override string ToString() => Type switch
    {
        OperandType.SignedInt => SignedInt.ToString(),
        OperandType.UnsignedInt => UnsignedInt.ToString(),
        OperandType.String => $"\"{Str}\"",
        OperandType.Label or OperandType.Register => Str,
        _ => throw new InvalidOperationException("ToString() on an Operand of type Unknown")
    };
}