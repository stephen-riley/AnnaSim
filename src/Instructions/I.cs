using System.Data;

namespace AnnaSim.Instructions;

public static class I
{
    public static readonly Dictionary<string, InstructionDefinition> Lookup = [];

    public static readonly int MathOpcode;

    static I()
    {
        var classes = typeof(InstructionDefinition)
            .Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(InstructionDefinition)) && !t.IsAbstract)
            .Select(t => (InstructionDefinition?)Activator.CreateInstance(t));

        foreach (var cls in classes)
        {
            var m = cls?.Mnemonic ?? throw new NoNullAllowedException();
            Lookup[m] = cls;
        }

        MathOpcode = Lookup["add"].Opcode;
    }

    public static InstructionDefinition GetIdef(uint w) => GetIdef((int)w >> 12, (int)w & 0x07);

    public static InstructionDefinition GetIdef(int opcode, int mathOp)
    {
        var defs = Lookup.Values.Where(id => id.Opcode == opcode).ToList();
        if (defs.Count > 1)
        {
            defs = defs.Where(id => id.MathOp == (MathOperation)mathOp).ToList();
        }
        return defs[0];
    }

    public static Instruction Instruction(ushort bits) => new Instruction(GetIdef(bits, bits));
}