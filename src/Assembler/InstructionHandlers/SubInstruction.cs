using AnnaSim.Assember;

namespace AnnaSim.Instructions.Definitions;

public partial class SubInstruction
{
    public override void Assemble(AnnaAssembler asm)
    {
        throw new NotImplementedException($"SubInstruction.{nameof(Assemble)}");
    }
}

