using AnnaSim.Assember;

namespace AnnaSim.Instructions.Definitions;

public partial class InInstruction
{
    public override void Assemble(AnnaAssembler asm)
    {
        throw new NotImplementedException($"InInstruction.{nameof(Assemble)}");
    }
}

