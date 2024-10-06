using AnnaSim.AsmParsing;
using CommandLine;

namespace AnnaSim.Cpu;

public class AnnaSimContext
{
    [Option("disasm", HelpText = "Save disassembly file")]
    public string? DisassemblyFilename { get; set; }

    [Option('r', "run", HelpText = "Run program instead of saving output")]
    public bool Run { get; set; }

    [Option('i', "input", HelpText = "Specifiy inputs for `in` instruction")]
    public IEnumerable<string> Inputs { get; set; } = null!;

    [Option('x', "debuggercmd", Hidden = true)]
    public IEnumerable<string> DebugCommands { get; set; } = null!;

    [Option('d', "debug", HelpText = "Debug program after assembling")]
    public bool Debug { get; set; }

    [Option('g', "optimize", HelpText = "Optimization level (0=none, 1=opt w/ comments, 2=optimize)")]
    public int OptimizationLevel { get; set; } = 2;

    [Option('a', "advanced-dbg", HelpText = "Debug program after assembling with VT100 debugger")]
    public bool AdvancedDebug { get; set; }

    [Option("save-asm", HelpText = "Save assembly file after C compile")]
    public string? SaveAsmFilename { get; set; }

    [Option('c', "cc", HelpText = "Compile C source")]
    public bool Cc { get; set; }

    [Option('m', "memory-image", HelpText = "Save memory image")]
    public string? MemoryFilename { get; set; }

    [Option('t', "trace", HelpText = "Trace output")]
    public bool Trace { get; set; }

    [Option('y', "max-cycles", HelpText = "Max cycles allowed to run program")]
    public int MaxCycles { get; set; } = 10_000;

    [Option("screendump", HelpText = "Dump the logical screen after execution")]
    public bool DumpScreen { get; set; }

    [Value(0, MetaName = "InputFilename", HelpText = "Input filename (if not specified, uses STDIN)")]
    public string? InputFilename { get; set; }

    public string Source { get; set; } = null!;
    public string? AsmSource { get; set; }
    public string? CSource { get; set; }
    public CstProgram CstProgram { get; set; } = null!;

    private static readonly UnParserSettings settings = new()
    {
        GroupSwitches = true,
        PreferShortName = true,
        UseEqualToken = false
    };

    // [Usage(ApplicationAlias = "annasim")]
    // public static IEnumerable<Example> Examples
    // {
    //     get
    //     {
    //         yield return new Example("Run an assembly file with two inputs", settings, new RunCliOptions { Filename = "myprogram.asm", Inputs = ["3", "0x0005"] });
    //         yield return new Example("Run a memory image, dumping the simulator screen after", settings, new RunCliOptions { Filename = "image.mem", DumpScreen = true });
    //         yield return new Example("Run an assembly file from STDIN", settings, new RunCliOptions { Inputs = ["3", "0x0005"] });
    //     }
    // }
}