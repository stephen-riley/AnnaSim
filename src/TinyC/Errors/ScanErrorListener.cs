using Antlr4.Runtime;

namespace AnnaSim.TinyC.Errors
{
    public class ScanErrorListener : ConsoleErrorListener<int>
    {
        public bool HadError => Exceptions.Any();

        public List<ScanException> Exceptions { get; } = new();

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line,
            int col, string msg, RecognitionException e)
        {
            Exceptions.Add(new ScanException(msg, offendingSymbol.ToString(), line, col));
            base.SyntaxError(output, recognizer, offendingSymbol, line, col, msg, e);
        }
    }
}