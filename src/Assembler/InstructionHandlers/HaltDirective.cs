using AnnaSim.Assember;

namespace AnnaSim.Instructions.Definitions;

public partial class HaltDirective
{
    public override void Assemble(AnnaAssembler asm)
    {
        throw new NotImplementedException($"HaltDirective.{nameof(Assemble)}");
    }
}

