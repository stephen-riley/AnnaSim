using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;
using AnnaSim.Instructions;

namespace AnnaSim.Debugger;

public abstract class BaseDebugger
{
    protected readonly CstProgram program;
    protected readonly uint[] origInputs = [];
    protected readonly string[] argv;
    protected readonly List<uint> watches = [];
    protected readonly List<string> terminalBuffer = [];
    protected readonly List<string> history = [];
    protected Word[] lastRegistersState = [];

    public Word ScreenMap { get; init; }
    public AnnaMachine Cpu { get; init; }
    public List<Word> Outputs { get; init; } = [];
    public HaltReason Status { get; private set; }

    protected BaseDebugger(CstProgram program, string[] inputs, int screenMap = 0xc000) : this(program, inputs, [], screenMap) { }
    protected BaseDebugger(CstProgram program, int screenMap = 0xc000) : this(program, [], [], screenMap) { }

    protected BaseDebugger(CstProgram program, string[] inputs, string[] argv, int screenMap = 0xc000)
    {
        this.program = program;
        origInputs = inputs.Select(AnnaMachine.ParseInputString).ToArray();
        this.argv = argv;
        ScreenMap = (uint)screenMap;

        Cpu = new AnnaMachine(program, origInputs)
        {
            OutputCallback = (w) =>
            {
                Outputs.Add(w);
                TerminalWriteLine($"out: {w}");
            },
            OutputStringCallback = TerminalWrite
        };
    }

    protected abstract void Prerun();

    protected abstract void Postrun();

    public IEnumerable<Word> Run(bool dumpScreen = false)
    {
        Prerun();

        var readFromConsole = argv.Length == 0;
        var cmdQueue = new Queue<string>(argv);

        DisplayBanner(program.Filename ?? "");

        Cpu.Reset();
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
                case 'b' when cmd.Length > 2:
                    uint breakAddr = 0;
                    var descriptor = "";

                    if (AnnaAssembler.TryParseNumeric(cmd[2..], out var numeric))
                    {
                        if (program.Filename?.EndsWith(".mem") ?? false)
                        {
                            breakAddr = (uint)numeric;
                            descriptor = $"address {numeric:x4}";
                        }
                        else
                        {
                            if (program.LineMap.ContainsKey((uint)numeric))
                            {
                                breakAddr = program.LineMap[(uint)numeric].BaseAddress;
                                descriptor = $"line {numeric}";
                            }
                            else
                            {
                                TerminalWriteLine($"* line {numeric} is not in PDB");
                                continue;
                            }
                        }
                    }
                    else if (program.Filename?.EndsWith(".mem") ?? false)
                    {
                        TerminalWriteLine($"* Cannot use labels on .mem files");
                        continue;
                    }
                    else
                    {
                        breakAddr = program.Labels[cmd[2..]];
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

                case 'c':
                    lastRegistersState = Cpu.Registers.Copy();
                    Status = Cpu.Execute();
                    break;

                case 'd':
                    DumpScreen();
                    break;

                case 'h': RenderHelp(); break;

                case 'i' when cmd.Length > 1:
                    var inputs = cmd[2..].Split();
                    Cpu.AddInputs(inputs);
                    TerminalWriteLine($"Added {inputs.Length} inputs");
                    break;

                case 'i':
                    Cpu.ClearInputs();
                    TerminalWriteLine("Cleared inputs.");
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
                        Cpu.Reset(origInputs);
                        TerminalWriteLine("Assembled file.");
                    }
                    break;

                case 'm' when cmd.Length > 2:
                    var addr = Convert.ToUInt16(cmd[2..], 16);
                    TerminalWriteLine($"* M[{cmd[2..]}]: {Cpu.Memory[addr]}");
                    break;

                case 'n':
                    lastRegistersState = Cpu.Registers.Copy();
                    Status = Cpu.ExecuteSingleInstruction();
                    break;

                case 'p':
                    TerminalWrite("Program symbols:");
                    foreach ((var label, var labelAddr) in program.Labels)
                    {
                        var line = program.LineMap.Where(el => el.Value.BaseAddress == labelAddr).Select(e => e.Key).FirstOrDefault();
                        var lineStr = line != 0 ? $" (line {line})" : "";
                        TerminalWrite($" 0x{labelAddr:x4} {label}{lineStr}");
                    }
                    TerminalWrite();
                    break;

                case 'q': goto breakLoop;

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

                case 'R':
                    if (program.Filename is null)
                    {
                        TerminalWriteLine("* cannot reset from STDIN");
                    }
                    else
                    {
                        Cpu.Reset(origInputs);
                        Outputs.Clear();
                        lastRegistersState = Cpu.Registers.Copy();
                        Status = HaltReason.Running;
                    }
                    break;

                case 's': TerminalWriteLine($"Processor status: {Cpu.Status}"); break;

                case 'w' when cmd.Length > 2:
                    addr = Convert.ToUInt16(cmd[2..], 16);
                    watches.Fluid(l => l.Add(addr)).Sort();
                    break;

                case 'W':
                    watches.Clear();
                    break;

                case 'x':
                    Cpu.Memory.WriteMemFile(cmd[2..]);
                    TerminalWriteLine($"Wrote image file to {cmd[2..]}");
                    break;

                default: TerminalWriteLine($"invalid command {cmd}"); break;
            }

            switch (Status)
            {
                case HaltReason.CyclesExceeded: TerminalWrite("> Allowed cycles exceeded"); break;
                case HaltReason.Halted: TerminalWrite($"> Halted, PC: 0x{Cpu.Pc:x4} ({Cpu.CyclesExecuted} cycles)"); break;
                case HaltReason.Breakpoint: TerminalWrite($"> Breakpoint, PC: 0x{Cpu.Pc:x4} ({Cpu.CyclesExecuted} cycles)"); break;
            }
        }

    breakLoop:
        TerminalWrite("Quitting.");

        if (dumpScreen)
        {
            DumpScreen();
        }

        Postrun();

        return Outputs;
    }

    protected void DumpScreen()
    {
        Console.WriteLine();
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
        TerminalWrite("c          Continue execution until halted");
        TerminalWrite("d          Dump screen memory");
        TerminalWrite("i [inputs] add Inputs to input queue");
        TerminalWrite("l [file]   Load .asm or .mem file");
        TerminalWrite("m          view Memory at address");
        TerminalWrite("n          execute Next instruction");
        TerminalWrite("p          Dump PDB information");
        TerminalWrite("q          Quit");
        TerminalWrite("rN         view Register N");
        TerminalWrite("r*         view all registers");
        TerminalWrite("w [addr]   Watch memory at addr");
        TerminalWrite("W          Clear watches");
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