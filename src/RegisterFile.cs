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
}