public class RegisterFile
{
    internal uint[] registers;

    public RegisterFile(int size = 8)
    {
        registers = new uint[size];
    }

    public uint this[uint n]
    {
        get { return n == 0 ? 0 : registers[n]; }
        set { registers[n] = value; }
    }
}