using AnnaSim.AsmParsing;
using AnnaSim.Extensions;
using AnnaSim.Instructions;

namespace AnnaSim.Debugger;

public class Vt100ConsoleDebugger : BaseDebugger
{
    public Vt100ConsoleDebugger(CstProgram program, string[] inputs, int screenMap = 0xc000) : this(program, inputs, [], screenMap) { }
    public Vt100ConsoleDebugger(CstProgram program, int screenMap = 0xc000) : this(program, [], [], screenMap) { }

    public Vt100ConsoleDebugger(CstProgram program, string[] inputs, string[] argv, int screenMap = 0xc000) : base(program, inputs, argv, screenMap) { }

    protected override void Prerun() => Console.Clear();

    protected override void Postrun() => Console.CursorVisible = true;

    protected override void UpdateScreen(Instruction? instr)
    {
        Console.CursorVisible = false;
        RenderScreenMap();
        RenderRegisters();
        RenderStack();
        RenderWatches();
        RenderInputs();
        RenderOutputs();
        RenderTerminal();
        RenderPrompt(instr);
        Console.CursorVisible = true;
    }

    private void RenderScreenMap()
    {
        Console.SetCursorPosition(0, 0);
        Console.Write("╔" + new string('═', 40) + "╗" + new string(' ', 38) + "\n║");
        for (var offset = 0u; offset < 40 * 25; offset++)
        {
            if (offset % 40 == 0 && offset != 0)
            {
                Console.Write("║" + new string(' ', 38) + "\n║");
            }
            byte ascii = (byte)(Cpu.Memory[ScreenMap + offset] & 0x7f);
            char c = ascii != 0 ? (char)ascii : ' ';
            Console.Write(c);
        }
        Console.Write("║\n╚" + new string('═', 40) + "╝\n");
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
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.Write($"r{r}: {Cpu.Registers[(uint)r]:x4}" + new string(' ', 10));
            if (Cpu.Registers[(uint)r] != lastRegistersState[r])
            {
                Console.ForegroundColor = defaultColor;
            }
        }

        Console.SetCursorPosition(46, 11);
        Console.Write($"PC: 0x{Cpu.Pc:x4}" + new string(' ', 10));
    }

    public void RenderStack()
    {
        Console.SetCursorPosition(59, 1);
        Console.Write("Stack");

        var defaultColor = Console.ForegroundColor;

        var baseAddr = Cpu.Registers[7] >= 0x7ffb ? 0x8000 : Cpu.Registers[7] + 5;
        for (int i = 0; i <= 10; i++)
        {
            var addr = baseAddr - i;
            var ptr = Cpu.Registers[7] == Cpu.Registers[6] && Cpu.Registers[6] == addr
                        ? " <- SP, FP"
                        : Cpu.Registers[7] == addr
                          ? " <- SP    "
                          : Cpu.Registers[6] == addr
                            ? " <- FP    "
                            : "          ";
            Console.SetCursorPosition(60, 2 + i);

            if (addr <= Cpu.Registers[7])
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }

            Console.Write($"0x{addr & 0xffff:x4}: 0x{Cpu.Memory[(uint)addr]:x4}");
            Console.ForegroundColor = defaultColor;
            Console.Write(ptr);
        }
    }

    private void RenderWatches()
    {
        Console.SetCursorPosition(45, 15);
        Console.Write("Watches");

        for (var i = 0; i < watches.Count; i++)
        {
            Console.SetCursorPosition(46, 16 + i);
            Console.Write($"0x{watches[i]:x4}:");
            for (uint offset = 0; offset < 4; offset++)
            {
                Console.Write($" {Cpu.Memory[watches[i] + offset]}");
            }
        }

        if (watches.Count == 0)
        {
            Console.SetCursorPosition(47, 16);
            Console.Write("(none)");
        }
    }

    private void RenderInputs()
    {
        Console.SetCursorPosition(45, 21);
        Console.Write("Inputs");
        Console.SetCursorPosition(47, 22);
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
        Console.SetCursorPosition(45, 24);
        Console.Write("Outputs");
        Console.SetCursorPosition(47, 25);
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
        const int terminalHeight = 15;

        List<string> terminalLines;

        if (terminalBuffer.Count < terminalHeight)
        {
            terminalLines = Enumerable.Repeat("", terminalHeight - terminalBuffer.Count).ToList().Fluid(l => l.AddRange(terminalBuffer));
        }
        else
        {
            terminalLines = terminalBuffer.TakeLast(terminalHeight).ToList();
        }

        for (var idx = 0; idx < terminalHeight; idx++)
        {
            Console.SetCursorPosition(1, 29 + idx);
            Console.Write(terminalLines[idx] + new string(' ', 80 - terminalLines[idx].Length));
        }
    }

    private void RenderPrompt(Instruction? instr, bool colorful = true)
    {
        Console.SetCursorPosition(1, 44);

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

    protected override string ReadDebuggerCommand()
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

    protected override void TerminalWrite(string s) => terminalBuffer.Add(s);

    private static void ConsoleClearEol()
    {
        var (col, row) = Console.GetCursorPosition();
        var width = Console.WindowWidth;
        Console.Write(new string(' ', width - col));
        Console.SetCursorPosition(col, row);
    }
}