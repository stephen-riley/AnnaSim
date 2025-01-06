using AnnaSim.TinyC.Antlr;
using Antlr4.Runtime;
using AnnaSim.TinyC.Errors;
using AnnaSim.TinyC.Extensions;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace AnnaSim.TinyC;

public class Compiler
{
    private readonly ScanErrorListener scanErrorListener = new();
    private readonly ParseErrorListener parseErrorListener = new();

    public bool Trace { get; set; }

    public bool Optimize { get; set; } = true;

    public bool ShowOptimizationComments { get; set; } = false;

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

    public static bool TryCompile(string csource, [NotNullWhen(true)] out string? asmSource, string? filename = null, bool showParseTree = false, int optimization = 2)
    {
        var compiler = new Compiler() { Trace = showParseTree, Optimize = optimization > 0, ShowOptimizationComments = optimization == 1 };
        var success = compiler.BuildParseTree(csource);

        if (success)
        {
            if (showParseTree)
            {
                var prettyTree = compiler.GetStringTree().ToPrettyParseTree();
                Console.Error.WriteLine();
                Console.Error.WriteLine($"# parse tree: {prettyTree}");
                Console.Error.WriteLine();
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

            var sched = Emitter.Emit(sa.Cc, compiler.ParseTree, filename ?? "(STDIN)");

            if (compiler.Optimize)
            {
                sched.Optimize(compiler.ShowOptimizationComments);
            }

            // TODO: replace with StringWriter?
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms) { AutoFlush = true };
            sched.Render(writer);
            asmSource = Encoding.UTF8.GetString(ms.ToArray());
            return true;
        }
        else
        {
            compiler.WriteErrors();
            asmSource = null;
            return false;
        }
    }
}