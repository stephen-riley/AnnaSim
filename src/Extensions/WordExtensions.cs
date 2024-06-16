using AnnaSim.Cpu.Instructions;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Extensions;

public static class WordExtensions
{
    public static Instruction ToInstruction(this Word w) => new(w);
}