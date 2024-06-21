using AnnaSim.Assember;

namespace AnnaSim.Instructions.Definitions;

public partial class BeqInstruction
{
    public override void Assemble(AnnaAssembler asm)
    {
        throw new NotImplementedException($"BeqInstruction.{nameof(Assemble)}");
    }
}

