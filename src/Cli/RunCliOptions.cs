using CommandLine;
using CommandLine.Text;

namespace AnnaSim.Cli;

[Verb("run", HelpText = "Run a program (.asm) or memory image (.mem)")]
public class RunCliOptions
{
    [Option('f', "filename", Default = "-", HelpText = "Filename (.asm or .mem); do not specify to read from STDIN")]
    public string Filename { get; set; } = "";

    [Option('i', "input", HelpText = "Specifiy inputs for `in` instruction")]
    public IEnumerable<string> Inputs { get; set; } = null!;

    [Option('s', "screendump", HelpText = "Dump screen to STDOUT after execution")]
    public bool DumpScreen { get; set; }

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
            yield return new Example("Run an assembly file with two inputs", settings, new RunCliOptions { Filename = "myprogram.asm", Inputs = ["3", "0x0005"] });
            yield return new Example("Run a memory image, dumping the simulator screen after", settings, new RunCliOptions { Filename = "image.mem", DumpScreen = true });
            yield return new Example("Run an assembly file from STDIN", settings, new RunCliOptions { Inputs = ["3", "0x0005"] });
        }
    }
}