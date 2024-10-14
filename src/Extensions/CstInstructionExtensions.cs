using AnnaSim.AsmParsing;
using static AnnaSim.Instructions.InstrOpcode;

namespace AnnaSim.Extensions;

public static class CstrInstructionExtensions
{
    public static IEnumerable<CstInstruction> ThatOccupyMemory(this IEnumerable<CstInstruction> cis)
    {
        return cis.Where(ci => ci.OccupiesMemory());
    }

    public static bool OccupiesMemory(this CstInstruction ci)
    {
        return ci.Opcode switch
        {
            _Org => false,
            _Def => false,
            _Ralias => false,
            _ => true
        };
    }
}