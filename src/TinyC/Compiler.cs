using AnnaSim.TinyC.Antlr;
using Antlr4.Runtime;
using AnnaSim.TinyC.Errors;

namespace AnnaSim.TinyC;

public class Compiler
{
    private readonly ScanErrorListener scanErrorListener = new();
    private readonly ParseErrorListener parseErrorListener = new();

    public bool Trace { get; set; }

    private AnnaCcParser parser = null!;

    // After a successful `Compile()` call, this property will have the root
    // tree node of your parse tree.
    public ParserRuleContext ParseTree { get; private set; } = null!;

    // Parse and build a parse tree for the input string.
    private bool BuildParseTree(string input)
    {
        // Set up ANTLR's inputs
        var str = new AntlrInputStream(input);
        var lexer = new AnnaCcLexer(str);
        var tokens = new CommonTokenStream(lexer);
        parser = new AnnaCcParser(tokens) { Trace = Trace };
        var lexerListener = new ScanErrorListener();
        var parserListener = new ParseErrorListener();
        lexer.AddErrorListener(lexerListener);
        parser.AddErrorListener(parserListener);

        // Parse the input
        ParseTree = parser.entrypoint();
        return !(lexerListener.HadError || parserListener.HadError);
    }

    // After a successful `Compile()` call, this gets the s-expression
    // tree string.
    public string GetStringTree()
    {
        return ParseTree?.ToStringTree(parser) ?? "ERR";
    }

    // Dump any errors to the console.
    public void WriteErrors()
    {
        if (scanErrorListener.HadError)
        {
            Console.WriteLine("Lexer errors:");
            foreach (var err in scanErrorListener.Exceptions)
            {
                Console.WriteLine($"* {err}");
            }
        }

        if (parseErrorListener.HadError)
        {
            Console.WriteLine("Parser errors:");
            foreach (var err in parseErrorListener.Exceptions)
            {
                Console.WriteLine($"* {err}");
            }
        }
    }

    public static bool TryCompile(string input, out string? asmSource, bool showParseTree = false)
    {
        var compiler = new Compiler() { Trace = false };
        var success = compiler.BuildParseTree(input);

        if (success)
        {
            if (showParseTree)
            {
                Console.WriteLine($"# parse tree: {compiler.GetStringTree()}");
                Console.WriteLine();
            }

            var sa = SemanticAnalyzer.Evaluate(compiler.ParseTree);
            foreach (var e in sa.Errors)
            {
                Console.Error.WriteLine(e);
            }

            if (sa.HasErrors)
            {
                asmSource = null;
                return false;
            }

            Emitter.Emit(sa.Cc, compiler.ParseTree);
        }
        else
        {
            compiler.WriteErrors();
            asmSource = null;
            return false;
        }

        // TODO: have the emitter write to a data structure, then render it to this string
        asmSource = "# TBD";
        return true;
    }
}