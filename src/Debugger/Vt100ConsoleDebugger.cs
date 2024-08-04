using AnnaSim.Cpu;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;
using AnnaSim.Instructions;

namespace AnnaSim.Debugger;

public class Vt100ConsoleDebugger
{
    private readonly uint[] origInputs = [];
    private readonly string fname;
    private readonly string[] argv;
    private List<uint> watches = [];

    public Word ScreenMap { get; init; }
    public AnnaMachine Cpu { get; init; }
    public List<Word> Outputs { get; init; } = [];
    public HaltReason Status { get; private set; }

    public Vt100ConsoleDebugger(string fname, string[] inputs, string[] argv, int screenMap = 0x8000)
    {
        this.fname = fname;
        origInputs = inputs.Select(AnnaMachine.ParseInputString).ToArray();
        this.argv = argv;
        ScreenMap = (uint)screenMap;

        Cpu = new AnnaMachine(fname, origInputs)
        {
            OutputCallback = (w) =>
            {
                Outputs.Add(w);
                Console.WriteLine($"out: {w}");
            }
        };
    }

    public Vt100ConsoleDebugger(string fname, string[] inputs, int screenMap = 0x8000) : this(fname, inputs, [], screenMap) { }
    public Vt100ConsoleDebugger(string fname, int screenMap = 0x8000) : this(fname, [], [], screenMap) { }

    public IEnumerable<Word> Run()
    {
        Console.Clear();
        Console.SetCursorPosition(0, 29);
        DisplayBanner(fname);

        var readFromConsole = argv.Length == 0;
        var cmdQueue = new Queue<string>(argv);

        while (true)
        {
            Console.WriteLine();

            var instr = I.Instruction(Cpu.Memory[Cpu.Pc]);
            Prompt(instr);
            var (col, row) = Console.GetCursorPosition();
            RenderScreen();
            Console.SetCursorPosition(col, row);

            string cmd;
            if (readFromConsole)
            {
                cmd = Console.ReadLine() ?? "";
            }
            else if (cmdQueue.TryDequeue(out var next))
            {
                cmd = next ?? "";
                Console.WriteLine(cmd);
            }
            else
            {
                return Outputs;
            }

            if (cmd == "q")
            {
                break;
            }
            else if (cmd == "c")
            {
                Status = Cpu.Execute();
            }
            else if (cmd.StartsWith('r'))
            {
                // TODO: add register aliases to assembler output

                var r = Convert.ToInt32(cmd[1..]);
                if (r < 0 || (r >= Cpu.Registers.Length))
                {
                    Console.WriteLine($"* invalid register {cmd} (must be in the range 0..{Cpu.Registers.Length - 1})");
                    continue;
                }

                Console.WriteLine($"* {cmd}: {Cpu.Registers[Convert.ToUInt16(cmd[1..])]}");
            }
            else if (cmd.StartsWith("m "))
            {
                var addr = Convert.ToUInt16(cmd[2..], 16);
                Console.WriteLine($"* M[{cmd[2..]}]: {Cpu.Memory[addr]}");
            }
            else if (cmd.StartsWith("w "))
            {
                var addr = Convert.ToUInt16(cmd[2..], 16);
                watches.Fluid(l => l.Add(addr)).Sort();
            }
            else if (cmd == "R")
            {
                Cpu.Reset(origInputs);
                Status = HaltReason.Running;
            }
            else if (cmd is "" or "n")
            {
                Status = Cpu.ExecuteSingleInstruction();
            }
            else if (cmd == "s")
            {
                Console.WriteLine($"Processor status: {Cpu.Status}");
            }
            else
            {
                Console.WriteLine($"invalid command {cmd}");
            }

            switch (Status)
            {
                case HaltReason.CyclesExceeded:
                    Console.WriteLine("Allowed cycles exceeded");
                    break;
                case HaltReason.Halt:
                    Console.WriteLine($"Halted, PC: 0x{Cpu.Pc:x4}");
                    break;
                case HaltReason.Breakpoint:
                    Console.WriteLine($"Breakpoint, PC: 0x{Cpu.Pc:x4}");
                    break;
            }
        }

        Console.WriteLine("Quitting.");
        return Outputs;
    }

    private void RenderScreen()
    {
        Console.SetCursorPosition(0, 0);
        Console.Write("+" + new string('-', 40) + "+" + new string(' ', 38) + "\n|");
        for (var offset = 0u; offset < 40 * 25; offset++)
        {
            if (offset % 40 == 0 && offset != 0)
            {
                Console.Write("|" + new string(' ', 38) + "\n|");
            }
            byte ascii = (byte)(Cpu.Memory[ScreenMap + offset] & 0x7f);
            char c = ascii != 0 ? (char)ascii : ' ';
            Console.Write(c);
        }
        Console.Write("|\n+" + new string('-', 40) + "+\n");
        Console.WriteLine(new string(' ', 42));

        Console.SetCursorPosition(45, 1);
        Console.Write("Registers");
        for (var r = 0; r < Cpu.Registers.Length; r++)
        {
            Console.SetCursorPosition(46, 2 + r);
            Console.Write($"r{r}: {Cpu.Registers[(uint)r]:x4}");
        }

        Console.SetCursorPosition(45, 12);
        Console.Write("Watches");
        for (var i = 0; i < watches.Count; i++)
        {
            Console.SetCursorPosition(46, 13 + i);
            Console.Write($"0x{watches[i]:x4}:");
            for (uint offset = 0; offset < 4; offset++)
            {
                Console.Write($" {Cpu.Memory[watches[i] + offset]}");
            }
        }
    }

    public static void DisplayBanner(string fname)
    {
        Console.WriteLine("AnnaSim Debugger");
        Console.WriteLine($"  running {fname}");
    }

    public void Prompt(Instruction instr)
    {
        var cachedForeground = Console.ForegroundColor;
        var cachedBackground = Console.BackgroundColor;
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.Write($"PC:0x{Cpu.Pc:x4} ({instr}) |> ");
        Console.ForegroundColor = cachedForeground;
        Console.BackgroundColor = cachedBackground;
    }
}