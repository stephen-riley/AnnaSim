namespace AnnaSim.TinyC.Errors;

public class ScanException : Exception
{
    public ScanException(string msg, string offendingSymbol, int line, int col) : base($"{msg} - offending symbol \"{offendingSymbol}\" ({line}:{col})") { }
}
