using CommandLine;
using CommandLine.Text;

namespace AnnaSim.Cli;

[Verb("debug", HelpText = "Debug a program (.asm) or memory image (.mem) with an interactive console")]
public class DebugCliOptions
{
    [Option('f', "filename", Default = "-", HelpText = "Filename (.asm or .mem); do not specify to read from STDIN")]
    public string Filename { get; set; } = "";

    [Option('i', "input", HelpText = "Specifiy inputs for `in` instruction")]
    public IEnumerable<string> Inputs { get; set; } = null!;

    [Option('c', "debuggercmd", Hidden = true)]
    public IEnumerable<string> DebugCommands { get; set; } = null!;

    [Option('v', "vt100", HelpText = "Use the \"pretty\" debugger")]
    public bool Vt100 { get; set; }

    [Option('s', "screendump", HelpText = "Dump screen to STDOUT after execution")]
    public bool DumpScreen { get; set; }

    [Option("dump-disassembly", HelpText = "Dump the disassembled form")]
    public string DumpFilename { get; set; } = "";

    private static readonly UnParserSettings settings = new()
    {
        GroupSwitches = true,
        PreferShortName = true,
        UseEqualToken = false
    };

    [Usage(ApplicationAlias = "annasim")]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("Debug an assembly file with two inputs", settings, new DebugCliOptions { Filename = "myprogram.asm", Inputs = ["3", "0x0005"] });
            yield return new Example("Debug a memory image, dumping the simulator screen after", settings, new DebugCliOptions { Filename = "image.mem", DumpScreen = true });
            yield return new Example("Debug an assembly file with the nice debugger", settings, new DebugCliOptions { Filename = "myprogram.asm", Vt100 = true });
        }
    }
}