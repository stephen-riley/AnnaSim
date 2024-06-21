using AnnaSim.Assember;

namespace AnnaSim.Instructions.Definitions;

public partial class OutInstruction
{
    public override void Assemble(AnnaAssembler asm)
    {
        throw new NotImplementedException($"OutInstruction.{nameof(Assemble)}");
    }
}

