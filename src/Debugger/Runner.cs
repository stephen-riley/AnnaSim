using AnnaSim.Cpu;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Debugger;

// Worst name ever: this is really just a thin wrapper around AnnaMachine.Execute()

public class Runner
{
    protected readonly uint[] origInputs = [];
    protected readonly string origFilename;

    public Word ScreenMap { get; init; }
    public AnnaMachine Cpu { get; init; }
    public List<Word> Outputs { get; init; } = [];
    public HaltReason Status { get; private set; }

    public Runner(string fname, string[] inputs, int screenMap = 0xc000)
    {
        origFilename = fname;
        origInputs = inputs.Select(AnnaMachine.ParseInputString).ToArray();
        ScreenMap = (uint)screenMap;

        Cpu = new AnnaMachine(fname, origInputs)
        {
            OutputCallback = Outputs.Add
        };
    }

    public IEnumerable<Word> Run(bool dumpScreen)
    {
        Cpu.Reset(origFilename);
        Cpu.Status = CpuStatus.Running;

        Status = Cpu.Execute();

        Console.WriteLine($"{Status.ToString()} at PC: 0x{Cpu.Pc:x4} ({Cpu.CyclesExecuted} cycles)");

        if (dumpScreen)
        {
            DumpScreen();
        }

        Console.Write("Outputs: ");

        if (Outputs.Count > 0)
        {
            Console.WriteLine(string.Join(" ", Outputs.Select(o => $"{o:x4}")));
        }
        else
        {
            Console.WriteLine("(none)");
        }

        return Outputs;
    }

    public void DumpScreen()
    {
        Console.WriteLine("+" + new string('-', 40) + "+");
        for (var row = 0; row < 25; row++)
        {
            var rowStr = Enumerable.Range(0, 40)
                .Select(i => (uint)(ScreenMap + i + row * 40))
                .Select(addr => Cpu.Memory[addr] != 0 ? (char)Cpu.Memory[addr] : ' ');

            Console.WriteLine("|" + string.Join("", rowStr) + "|");
        }
        Console.WriteLine("+" + new string('-', 40) + "+");
        Console.WriteLine();
    }
}