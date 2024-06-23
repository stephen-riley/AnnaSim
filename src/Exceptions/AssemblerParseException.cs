namespace AnnaSim.Exceptions;

public class AssemblerParseException : Exception
{
    static string ConstructMessage(string line) => $"could not parse line [{line}]";

    public AssemblerParseException(string line) : base(ConstructMessage(line)) { }

    public AssemblerParseException(string line, Exception inner) : base(ConstructMessage(line), inner) { }
}