using AnnaSim.Cpu;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;
using AnnaSim.Instructions;

namespace AnnaSim.Debugger;

public abstract class BaseDebugger
{
    protected readonly uint[] origInputs = [];
    protected readonly string fname;
    protected readonly string[] argv;
    protected readonly List<uint> watches = [];
    protected readonly List<string> terminalBuffer = [];
    protected readonly List<string> history = [];
    protected Word[] lastRegistersState = [];

    public Word ScreenMap { get; init; }
    public AnnaMachine Cpu { get; init; }
    public List<Word> Outputs { get; init; } = [];
    public HaltReason Status { get; private set; }

    protected BaseDebugger(string fname, string[] inputs, int screenMap = 0xc000) : this(fname, inputs, [], screenMap) { }
    protected BaseDebugger(string fname, int screenMap = 0xc000) : this(fname, [], [], screenMap) { }

    protected BaseDebugger(string fname, string[] inputs, string[] argv, int screenMap = 0xc000)
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

        Cpu.Reset(fname);
        Cpu.Status = CpuStatus.Running;

        lastRegistersState = Cpu.Registers.Copy();

        while (true)
        {
            var instr = ISA.Instruction(Cpu.MemoryAtPc);
            UpdateScreen(instr);

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

                case 'r' when cmd == "r*":
                    var display = string.Join(" | ", Enumerable.Range(0, Cpu.Registers.Length)
                        .Select(r => $"r{r}: {Cpu.Registers[(uint)r]:x4}"));
                    TerminalWriteLine($"* {display}");
                    break;

                case 'r' when cmd.Length > 1:
                    // TODO: add register aliases to assembler output

                    uint r = Convert.ToUInt32(cmd[1..]);
                    if (r < 0 || (r >= Cpu.Registers.Length))
                    {
                        TerminalWriteLine($"* invalid register {cmd} (must be in the range 0..{Cpu.Registers.Length - 1})");
                        continue;
                    }
                    TerminalWriteLine($"* r{r}: {Cpu.Registers[r]}");
                    break;

                case 'r':
                    TerminalWriteLine("* must specify a register or register alias");
                    break;

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

                case 'l':
                    var fname = cmd[2..];
                    if (fname.EndsWith(".mem"))
                    {
                        Cpu.Reset();
                        Cpu.ReadMemFile(fname);
                        TerminalWriteLine("Loaded memory file.");
                    }
                    else
                    {
                        Cpu.Reset(fname, origInputs);
                        TerminalWriteLine("Assembled file.");
                    }
                    break;

                case 'i' when cmd.Length > 1:
                    var inputs = cmd[2..].Split();
                    Cpu.AddInputs(inputs);
                    TerminalWriteLine($"Added {inputs.Length} inputs");
                    break;

                case 'i':
                    Cpu.ClearInputs();
                    TerminalWriteLine("Cleared inputs.");
                    break;

                case 'd':
                    DumpScreen();
                    break;

                case 'h': RenderHelp(); break;

                default: TerminalWriteLine($"invalid command {cmd}"); break;
            }

            switch (Status)
            {
                case HaltReason.CyclesExceeded: TerminalWrite("> Allowed cycles exceeded"); break;
                case HaltReason.Halt: TerminalWrite($"> Halted, PC: 0x{Cpu.Pc:x4}"); break;
                case HaltReason.Breakpoint: TerminalWrite($"> Breakpoint, PC: 0x{Cpu.Pc:x4}"); break;
            }
        }

    breakLoop:
        TerminalWrite("Quitting.");
        return Outputs;
    }

    protected void DumpScreen()
    {
        TerminalWrite("+" + new string('-', 40) + "+");
        for (var row = 0; row < 25; row++)
        {
            var rowStr = Enumerable.Range(0, 40)
                .Select(i => (uint)(ScreenMap + i + row * 40))
                .Select(addr => Cpu.Memory[addr] != 0 ? (char)Cpu.Memory[addr] : ' ');

            TerminalWrite("|" + string.Join("", rowStr) + "|");
        }
        TerminalWriteLine("+" + new string('-', 40) + "+");
    }

    protected abstract string ReadDebuggerCommand();

    protected abstract void UpdateScreen(Instruction? instr);

    protected void DisplayBanner(string fname)
    {
        TerminalWrite("AnnaSim Debugger");
        if (fname != string.Empty)
        {
            TerminalWrite($"  running {fname}");
        }
        TerminalWrite("");
        TerminalWriteLine("h to display help.");
    }

    protected string PromptString(Instruction? instr) => $"PC:0x{Cpu.Pc:x4} ({instr?.ToString() ?? ""}) |>";

    protected void RenderHelp()
    {
        TerminalWrite("h          Help");
        TerminalWrite("b [line]   set or clear Breakpoint at addr");
        TerminalWrite("b [sym]    set Breakpoint at symbol");
        TerminalWrite("c          Continue execute until halted");
        TerminalWrite("d          Dump screen memory");
        TerminalWrite("i [inputs] add Inputs to input queue");
        TerminalWrite("l [file]   Load .asm or .mem file");
        TerminalWrite("m          view Memory at address");
        TerminalWrite("n          execute Next instruction");
        TerminalWrite("q          Quit");
        TerminalWrite("rN         view Register N");
        TerminalWrite("w [addr]   Watch memory at addr");
        TerminalWrite();
    }

    protected abstract void TerminalWrite(string s);

    protected void TerminalWrite() => TerminalWrite("");

    protected void TerminalWriteLine(string s)
    {
        TerminalWrite(s);
        TerminalWrite("");
    }
}