namespace AnnaSim.Extensions;

public class AnnaStringWriter : StreamWriter
{
    private MemoryStream ms = new();

    public AnnaStringWriter() : this(new MemoryStream())
    {
    }

    public AnnaStringWriter(MemoryStream ms) : base(ms)
    {
        this.ms = ms;
        AutoFlush = true;
    }

    public override string ToString() => System.Text.Encoding.UTF8.GetString(ms.ToArray());
}