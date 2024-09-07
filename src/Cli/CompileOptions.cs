using CommandLine;
using CommandLine.Text;

namespace AnnaSim.Cli;

[Verb("cc", HelpText = "Compile a C program to ANNA assembly")]
public class CompileCliOptions
{
    [Option('f', "filename", Default = "-", HelpText = "Input filename (.c); do not specify to read from STDIN")]
    public string Filename { get; set; } = "";

    [Option('o', "output", Default = "-", HelpText = "Output filename (.asm); do not specify to output to STDOUT")]
    public string Output { get; set; } = "";

    [Option('d', "debug", HelpText = "Debug after compiling")]
    public bool Debug { get; set; }

    [Option('v', "vt100", HelpText = "Debug using the \"pretty\" debugger after compiling")]
    public bool Vt100 { get; set; }

    [Option('r', "run", HelpText = "Run the program after compiling")]
    public bool Run { get; set; }

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
            yield return new Example("Compile a .c file to a .asm file", settings, new CompileCliOptions { Filename = "example.c", Output = "example.asm" });
            yield return new Example("Compile a .c file piped from STDIN to a .asm file", settings, new CompileCliOptions { Output = "example.asm" });
            yield return new Example("Compile a .c file STDOUT", settings, new CompileCliOptions { Filename = "example.c" });

            yield return new Example("Run a .c file after compiling with an input", settings, new CompileCliOptions { Filename = "example.c", Run = true, Inputs = ["5"] });
            yield return new Example("Debug a .c file after compiling the \"pretty\" debugger", settings, new CompileCliOptions { Filename = "example.c", Vt100 = true, Inputs = ["5"] });
        }
    }
}