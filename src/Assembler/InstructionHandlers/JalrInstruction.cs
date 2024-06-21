using AnnaSim.Assember;

namespace AnnaSim.Instructions.Definitions;

public partial class JalrInstruction
{
    public override void Assemble(AnnaAssembler asm)
    {
        throw new NotImplementedException($"JalrInstruction.{nameof(Assemble)}");
    }
}

