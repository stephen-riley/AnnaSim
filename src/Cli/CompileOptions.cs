using CommandLine;
using CommandLine.Text;

namespace AnnaSim.Cli;

[Verb("cc", HelpText = "Compile a C program to ANNA assembly")]
public class CompileOptions
{
    [Option('f', "filename", Default = "-", HelpText = "Input filename (.c); do not specify to read from STDIN")]
    public string Filename { get; set; } = "";

    [Option('o', "output", Default = "-", HelpText = "Output filename (.asm); do not specify to output to STDOUT")]
    public string Output { get; set; } = "";

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
            yield return new Example("Compile a .c file to a .asm file", settings, new CompileOptions { Filename = "example.c", Output = "example.asm" });
            yield return new Example("Compile a .c file piped from STDIN to a .asm file", settings, new CompileOptions { Output = "example.asm" });
            yield return new Example("Compile a .c file STDOUT", settings, new CompileOptions { Filename = "example.c" });
        }
    }
}