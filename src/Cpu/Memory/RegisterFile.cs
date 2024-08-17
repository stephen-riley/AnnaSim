using System.Text;

namespace AnnaSim.Cpu.Memory;

public class RegisterFile
{
    internal Word[] registers;

    public int Length => registers.Length;

    public RegisterFile(int size = 8)
    {
        registers = new Word[size];
    }

    public Word this[uint n]
    {
        get { return n == 0 ? (Word)0 : registers[n]; }
        set { registers[n] = value; }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        for (int r = 0; r < registers.Length; r++)
        {
            sb.Append($"r{r}:");
            sb.Append(registers[r].ToString());
            sb.Append(' ');
        }

        return sb.ToString();
    }

    public Word[] Copy()
    {
        return (Word[])registers.Clone();
    }
}