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
    private readonly List<uint> watches = [];
    private readonly List<string> terminalBuffer = [];
    private readonly List<string> history = [];
    private Word[] lastRegistersState = [];

    public Word ScreenMap { get; init; }
    public AnnaMachine Cpu { get; init; }
    public List<Word> Outputs { get; init; } = [];
    public HaltReason Status { get; private set; }

    public Vt100ConsoleDebugger(string fname, string[] inputs, int screenMap = 0x8000) : this(fname, inputs, [], screenMap) { }
    public Vt100ConsoleDebugger(string fname, int screenMap = 0x8000) : this(fname, [], [], screenMap) { }

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
                TerminalWriteLine($"out: {w}");
            }
        };
    }

    public IEnumerable<Word> Run()
    {
        var readFromConsole = argv.Length == 0;
        var cmdQueue = new Queue<string>(argv);

        Console.Clear();
        DisplayBanner(fname);

        Cpu.Reset();
        Cpu.Status = CpuStatus.Running;

        lastRegistersState = Cpu.Registers.Copy();

        while (true)
        {
            var instr = ISA.Instruction(Cpu.MemoryAtPc);
            RenderScreen(instr);

            cmdQueue.TryDequeue(out string? cmd);
            cmd ??= ReadDebuggerCommand();

            if (cmd is not null)
            {
                cmd = cmd.Length > 0 ? cmd : "n";
                history.Add(cmd);
                TerminalWriteLine($"{PromptString(instr)} {cmd}");
            }
            else
            {
                return Outputs;
            }

            switch (cmd[0])
            {
                case 'q': goto breakLoop;
                case 'c':
                    lastRegistersState = Cpu.Registers.Copy();
                    Status = Cpu.Execute();
                    break;
                case 'r' when cmd.Length > 1:
                    // TODO: add register aliases to assembler output

                    var r = Convert.ToInt32(cmd[1..]);
                    if (r < 0 || (r >= Cpu.Registers.Length))
                    {
                        TerminalWriteLine($"* invalid register {cmd} (must be in the range 0..{Cpu.Registers.Length - 1})");
                        continue;
                    }
                    break;
                case 'r':
                    TerminalWriteLine("* must specify a register or register alias"); break;
                case 'm' when cmd.Length > 2:
                    var addr = Convert.ToUInt16(cmd[2..], 16);
                    TerminalWriteLine($"* M[{cmd[2..]}]: {Cpu.Memory[addr]}");
                    break;
                case 'w' when cmd.Length > 2:
                    addr = Convert.ToUInt16(cmd[2..], 16);
                    watches.Fluid(l => l.Add(addr)).Sort();
                    break;
                case 'R':
                    Cpu.Reset(origInputs);
                    Outputs.Clear();
                    lastRegistersState = Cpu.Registers.Copy();
                    Status = HaltReason.Running;
                    break;
                case 'n':
                    lastRegistersState = Cpu.Registers.Copy();
                    Status = Cpu.ExecuteSingleInstruction();
                    break;
                case 's': TerminalWriteLine($"Processor status: {Cpu.Status}"); break;
                case 'b' when cmd.Length > 2:
                    uint breakAddr = 0;
                    var descriptor = "";

                    if (int.TryParse(cmd[2..], out var lineNo))
                    {
                        breakAddr = Cpu.Pdb.LineAddrMap[lineNo];
                        descriptor = $"line {lineNo}";
                    }
                    else
                    {
                        breakAddr = Cpu.Pdb.Labels[cmd[2..]];
                        descriptor = $"label {cmd[2..]}";
                    }

                    if (Cpu.Memory.Get32bits(breakAddr).IsBreakpoint)
                    {
                        Cpu.Memory.ClearBreakpoint(breakAddr);
                        TerminalWriteLine($"Cleared breakpoint at {descriptor} (addr 0x{breakAddr:x4})");
                    }
                    else
                    {
                        Cpu.Memory.SetBreakpoint(breakAddr);
                        TerminalWriteLine($"Set breakpoint at {descriptor} (addr 0x{breakAddr:x4})");
                    }

                    break;

                case 'h': RenderHelp(); break;
                default: TerminalWriteLine($"invalid command {cmd}"); break;
            }

            switch (Status)
            {
                case HaltReason.CyclesExceeded: TerminalWrite("> Allowed cycles exceeded"); break;
                case HaltReason.Halt: TerminalWrite($"> Halted, PC: 0x{Cpu.Pc:x4}"); break;
                case HaltReason.Breakpoint: TerminalWrite($"Breakpoint, PC: 0x{Cpu.Pc:x4}"); break;
            }
        }

    breakLoop:
        TerminalWrite("Quitting.");
        return Outputs;
    }

    private void RenderScreenMap()
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
    }

    private void RenderRegisters()
    {
        var defaultColor = Console.ForegroundColor;

        Console.SetCursorPosition(45, 1);
        Console.Write("Registers");

        for (var r = 0; r < Cpu.Registers.Length; r++)
        {
            Console.SetCursorPosition(46, 2 + r);
            if (Cpu.Registers[(uint)r] != lastRegistersState[r])
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.Write($"r{r}: {Cpu.Registers[(uint)r]:x4}" + new string(' ', 10));
            if (Cpu.Registers[(uint)r] != lastRegistersState[r])
            {
                Console.ForegroundColor = defaultColor;
            }
        }
    }

    private void RenderWatches()
    {
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

        if (watches.Count == 0)
        {
            Console.SetCursorPosition(47, 13);
            Console.Write("(none)");
        }
    }

    private void RenderInputs()
    {
        Console.SetCursorPosition(45, 19);
        Console.Write("Inputs");
        Console.SetCursorPosition(49, 20);
        if (Cpu.Inputs.Count > 0)
        {
            Console.Write(string.Join(", ", Cpu.Inputs));
            ConsoleClearEol();
        }
        else
        {
            Console.Write("(none)");
            ConsoleClearEol();
        }
    }

    private void RenderOutputs()
    {
        Console.SetCursorPosition(45, 22);
        Console.Write("Outputs");
        Console.SetCursorPosition(49, 23);
        if (Outputs.Count > 0)
        {
            Console.Write(string.Join(", ", Outputs));
        }
        else
        {
            Console.Write("(none)");
            ConsoleClearEol();
        }
    }

    private void RenderTerminal()
    {
        const int terminalHeight = 10;

        List<string> terminalLines;

        if (terminalBuffer.Count < terminalHeight)
        {
            terminalLines = Enumerable.Repeat("", terminalHeight - terminalBuffer.Count).ToList().Fluid(l => l.AddRange(terminalBuffer));
        }
        else
        {
            terminalLines = terminalBuffer.TakeLast(10).ToList();
        }

        for (var idx = 0; idx < terminalHeight; idx++)
        {
            Console.SetCursorPosition(1, 29 + idx);
            Console.Write(terminalLines[idx] + new string(' ', 80 - terminalLines[idx].Length));
        }
    }

    private void RenderScreen(Instruction? instr)
    {
        Console.CursorVisible = false;
        RenderScreenMap();
        RenderRegisters();
        RenderWatches();
        RenderInputs();
        RenderOutputs();
        RenderTerminal();
        RenderPrompt(instr);
        Console.CursorVisible = true;
    }

    private void DisplayBanner(string fname)
    {
        terminalBuffer.Add("AnnaSim Debugger");
        if (fname != string.Empty)
        {
            terminalBuffer.Add($"  running {fname}");
        }
        terminalBuffer.Add("");
    }

    private string PromptString(Instruction? instr) => $"PC:0x{Cpu.Pc:x4} ({instr?.ToString() ?? ""}) |>";

    private void RenderPrompt(Instruction? instr, bool colorful = true)
    {
        Console.SetCursorPosition(1, 39);

        Console.CursorVisible = false;

        var cachedForeground = Console.ForegroundColor;
        var cachedBackground = Console.BackgroundColor;

        if (colorful)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Blue;
        }
        Console.Write(PromptString(instr));

        if (colorful)
        {
            Console.ForegroundColor = cachedForeground;
            Console.BackgroundColor = cachedBackground;
        }

        ConsoleClearEol();
        Console.Write(' ');
        Console.CursorVisible = true;
    }

    private static string ReadDebuggerCommand()
    {
        string cmd = string.Empty;
        while (true)
        {
            var (left, top) = Console.GetCursorPosition();

            var keyInfo = Console.ReadKey();

            if (keyInfo.Key == ConsoleKey.Backspace && cmd != string.Empty)
            {
                Console.SetCursorPosition(left - 1, top);
                Console.Write(' ');
                Console.SetCursorPosition(left - 1, top);

                cmd = cmd[0..^1];
            }
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.CursorVisible = false;
                return cmd;
            }
            else
            {
                cmd += keyInfo.KeyChar;
            }
        }
    }

    private void RenderHelp()
    {
        TerminalWrite("h          Help");
        TerminalWrite("b [line]   set or clear Breakpoint at addr");
        TerminalWrite("b [sym]    set Breakpoint at symbol");
        TerminalWrite("c          Continue execute until halted");
        TerminalWrite("m          view Memory at address");
        TerminalWrite("n          execute Next instruction");
        TerminalWrite("q          Quit");
        TerminalWrite("rN         view Register N");
        TerminalWrite("w [addr]   Watch memory at addr");
        TerminalWrite();
    }

    private void TerminalWrite(string s) => terminalBuffer.Add(s);

    private void TerminalWrite() => TerminalWrite("");

    private void TerminalWriteLine(string s)
    {
        TerminalWrite(s);
        TerminalWrite("");
    }

    private static void ConsoleClearEol()
    {
        var (col, row) = Console.GetCursorPosition();
        var width = Console.WindowWidth;
        Console.Write(new string(' ', width - col));
        Console.SetCursorPosition(col, row);
    }

}