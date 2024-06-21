using System.Data;

namespace AnnaSim.Instructions;

public static class Instructions
{
    internal static readonly Dictionary<string, AbstractInstruction> instructions = [];

    static Instructions()
    {
        var classes = typeof(AbstractInstruction)
            .Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(AbstractInstruction)) && !t.IsAbstract)
            .Select(t => (AbstractInstruction?)Activator.CreateInstance(t));

        foreach (var cls in classes)
        {
            var m = cls?.Mnemonic ?? throw new NoNullAllowedException();
            instructions[m] = cls;
        }
    }
}