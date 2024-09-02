using AnnaSim.TinyC.Antlr;

namespace AnnaSim.TinyC;

public partial class Emitter : AnnaCcBaseVisitor<bool>
{
    private static void EmitProlog()
    {
        EmitHeaderComment("Register map:");
        EmitHeaderComment("  r1  scratch register");
        EmitHeaderComment("  r2  scratch register");
        EmitHeaderComment("  r3  scratch register for expressions results");
        EmitHeaderComment("  r4  result from function calls");
        EmitHeaderComment("  r5  function call destinations (jalr)");
        EmitHeaderComment("  r6  current frame pointer (FP)");
        EmitHeaderComment("  r7  stack pointer (SP)");

        EmitBlankLine();
        EmitHeaderComment(".text segment");
        EmitBlankLine();

        EmitHeaderComment("set up main() stack frame");
        // TODO: switch to register aliases
        EmitInstruction("lwi", ["r7", "&_stack"], "initialize SP (r7)");
        EmitInstruction("add", ["r6", "r7", "0"], "initialize FP (r6)");
        EmitBlankLine();
        EmitHeaderComment("start of main");
    }

    private static void EmitFunctionBodies(CompilerContext cc, Emitter e)
    {
        foreach (var (name, scope) in e.Cc.Functions)
        {
            e.Cc.CurrentScope = scope;

            var epExit = $"{name}_exit";

            EmitStackFrameComments(scope);

            EmitLabel(name);
            EmitInstruction("push", ["r6"], "push FP");
            EmitInstruction("push", ["r5"], "push return address");

            if (scope.Vars.Count > 0)
            {
                EmitInstruction("addi", ["r7", "r7", (-scope.Vars.Count).ToString()], $"create space for {scope.Vars.Count} local variables");
            }

            EmitLabel($"{name}_body");
            e.VisitBlock(scope.Body);

            // unwind stack and return
            EmitLabel(epExit);
            EmitInstruction("pop", ["r7", "r4"], "load function result -> r4");
            EmitInstruction("lw", ["r5", "r6", "-1"], "load return addr from FP-1");
            EmitInstruction("lw", ["r6", "r6", "0"], "restore previous FP");
            EmitInstruction("addi", ["r7", "r7", scope.FrameSize.ToString()], "collapse stack frame");
            EmitInstruction("jalr", ["r5", "r0"], "return from function");
        }
    }

    private static void EmitStackFrameComments(Scope scope)
    {
        var argList = string.Join(", ", scope.Args.Select(a => $"{a.Type} {a.Name}"));
        EmitComment($"function {scope.Name}({argList})");

        foreach (var a in scope.Args)
        {
            EmitComment($"FP+{a.Offset}  {a.Name}");
        }

        EmitComment("FP+0  previous FP");
        EmitComment("FP-1  return addr");

        foreach (var v in scope.Vars)
        {
            EmitComment($"FP{v.Offset}  {v.Name}");
        }
        EmitBlankLine();
    }

    private static void EmitGlobalVars(Emitter e)
    {
        foreach (var v in e.Cc.GlobalScope.Vars)
        {
            EmitInstruction(label: $"_var_{v.Name}", op: ".fill", operands: ["0"], $"global variable {v.Name}");
        }
    }

    private static void EmitInternedStrings(CompilerContext cc)
    {
        foreach (var s in cc.InternedStrings)
        {
            EmitInstruction(label: s.Key, op: ".cstr", operands: [$"\"{s.Value}\""], "interned string");
        }
    }

    private static void EmitBlankLine() => Console.WriteLine();

    private static void EmitComment(string comment) => Console.WriteLine($"{new string(' ', 12)}# {comment}");

    private static void EmitHeaderComment(string comment) => Console.WriteLine($"# {comment}");

    private static void EmitLabel(string label) => Console.WriteLine($"{label}:");

    private static void EmitInstruction(string op, string[]? operands = null, string? comment = null) => EmitInstruction(null, op, operands, comment);

    private static void EmitInstruction(string? label = null, string? op = null, string[]? operands = null, string? comment = null)
    {
        var labelTerm = new string(' ', 12);
        var opTerm = new string(' ', 8);
        var operandsTerm = operands is null ? "" : string.Join(' ', operands);
        var commentTerm = comment is null ? "" : $"# {comment}";

        if (label != null)
        {
            labelTerm = $"{label + ':',-12}";
        }
        if (op != null)
        {
            opTerm = $"{op,-8}";
        }

        Console.WriteLine($"{labelTerm}{opTerm}{operandsTerm,-20}{commentTerm}");
    }

    private string GetInternedStringLabel(string str)
    {
        if (Cc.InternedStrings.TryGetValue(str, out var label))
        {
            return label;
        }
        else
        {
            var newLabel = $"_cstr{Cc.InternedStrings.Count:000}";
            Cc.InternedStrings[str] = newLabel;
            return newLabel;
        }
    }

    private string GetNextLabel(string root)
    {
        if (!labels.ContainsKey(root))
        {
            labels[root] = 0;
        }

        var n = labels[root];
        labels[root] = n + 1;
        return $"{root}_{n:n3}";
    }
}