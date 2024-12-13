using System.Diagnostics.CodeAnalysis;
using static AnnaSim.TinyC.Antlr.AnnaCcParser;

namespace AnnaSim.TinyC;

public class Scope
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public bool IsGlobal { get; set; }
    public BlockContext? Body { get; set; }
    public bool HasBody { get => Body is not null; }

    public List<Var> Args { get; } = [];
    public List<Var> Vars { get; } = [];

    public int FrameSize
    {
        // add 2 for FP and return addr
        get => Args.Count + Vars.Count + 2;
    }

    public override string ToString() => $"{(IsGlobal ? "global" : "function")} scope {(IsGlobal ? "" : Name)}";

    public void AddVar(string name, string type) => Vars.Add(new Var(name, type, -(Vars.Count + 2)));

    public void AddArg(string name, string type) => Args.Add(new Var(name, type, Args.Count + 1));

    public Var? GetByName(string name)
    {
        var a = Args.Where(a => a.Name == name).FirstOrDefault();
        if (a != null)
        {
            return a;
        }
        var v = Vars.Where(a => a.Name == name).FirstOrDefault();

        return v;
    }

    public bool TryGetByName(string name, [NotNullWhen(true)] out Var? scopeVar)
    {
        var res = GetByName(name);
        if (res is not null)
        {
            scopeVar = res;
            return true;
        }
        else
        {
            scopeVar = null;
            return false;
        }
    }

    public List<(string op, string[] operands, string comment)> GetLoadIntructions(string name, string targetRegister = "r1")
    {
        if (IsGlobal)
        {
            return [
                ("lwi", [targetRegister, $"&_var_{name}"], $"load address of variable {name}"),
                ("lw", ["r3", targetRegister, "0"], $"load variable \"{name}\" from data segment")
            ];
        }
        else
        {
            if (TryGetByName(name, out var varInfo))
            {
                var offsetDisplay = varInfo.Offset >= 0 ? $"+{varInfo.Offset}" : varInfo.Offset.ToString();
                return [("lw", ["r3", "rFP", varInfo.Offset.ToString()], $"load \"{name}\" from FP{offsetDisplay}")];
            }
            else
            {
                throw new InvalidOperationException($"cannot find variable \"{name}\" in {this}");
            }

        }
    }

    public List<(string op, string[] operands, string comment)> GetStoreIntructions(string name, string targetRegister = "r1")
    {
        if (IsGlobal)
        {
            return [
                ("lwi", [targetRegister, $"&_var_{name}"], $"load address of variable \"{name}\""),
                ("sw", ["r3", targetRegister, "0"], $"write r3 to variable \"{name}\" in data segment")
            ];
        }
        else
        {
            if (TryGetByName(name, out var varInfo))
            {
                var offsetDisplay = varInfo.Offset >= 0 ? $"+{varInfo.Offset}" : varInfo.Offset.ToString();
                return [("sw", ["r3", "rFP", varInfo.Offset.ToString()], $"write \"{name}\" to FP{offsetDisplay}")];
            }
            else
            {
                throw new InvalidOperationException($"cannot find variable {name} in {this}");
            }

        }
    }

    // done in the Antlr style when calling .GetText() on a rule (no spaces)
    public string VarsString => string.Join(",", Vars.Select(v => $"{v.Type}{v.Name}"));
    public string ArgsString => string.Join(",", Args.Select(v => $"{v.Type}{v.Name}"));

    public record Var(string Name, string Type, int Offset);
}