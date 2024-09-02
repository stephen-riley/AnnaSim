using Antlr4.Runtime;

namespace AnnaSim.TinyC.Errors;

public class ParseErrorListener : ConsoleErrorListener<IToken>
{
    public bool HadError => Exceptions.Any();

    public List<ParseException> Exceptions { get; } = new();

    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
        int col, string msg, RecognitionException e)
    {
        Exceptions.Add(new ParseException(msg, offendingSymbol?.ToString() ?? "ERR", line, col));

#pragma warning disable CS8604 // Possible null reference argument.
        base.SyntaxError(output, recognizer, offendingSymbol, line, col, msg, e);
#pragma warning restore CS8604 // Possible null reference argument.
    }
}
