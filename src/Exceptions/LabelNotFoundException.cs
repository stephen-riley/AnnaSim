namespace AnnaSim.Exceptions;

public class LabelNotFoundException : Exception
{
    public LabelNotFoundException(string label)
        : base($"Label {label} not found") { }
}