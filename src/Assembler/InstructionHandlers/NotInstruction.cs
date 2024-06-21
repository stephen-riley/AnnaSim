using AnnaSim.Assember;

namespace AnnaSim.Instructions.Definitions;

public partial class NotInstruction
{
    public override void Assemble(AnnaAssembler asm)
    {
        throw new NotImplementedException($"NotInstruction.{nameof(Assemble)}");
    }
}

