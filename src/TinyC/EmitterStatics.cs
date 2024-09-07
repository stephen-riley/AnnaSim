using System.Text.RegularExpressions;
using AnnaSim.TinyC.Antlr;
using AnnaSim.TinyC.Scheduler.Instructions;

namespace AnnaSim.TinyC;

public partial class Emitter : AnnaCcBaseVisitor<bool>
{
    private void EmitProlog(string filename)
    {
        EmitHeaderComment($"compiled from {filename}");
        EmitBlankLine();
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
        EmitInstruction(".org", ["0x0000"]);
        EmitBlankLine();

        EmitComment("set up main() stack frame");
        // TODO: switch to register aliases
        EmitInstruction("lwi", ["r7", "&_stack"], "initialize SP (r7)");
        EmitInstruction("add", ["r6", "r7", "r0"], "initialize FP (r6)");
        EmitBlankLine();
        EmitHeaderComment("start of main");
    }

    private void EmitFunctionBodies(CompilerContext cc, Emitter e)
    {
        foreach (var (name, scope) in e.Cc.Functions)
        {
            e.Cc.CurrentScope = scope;

            var epExit = $"{name}_exit";

            EmitStackFrameComments(scope);

            EmitLabel(name);
            EmitInstruction("mov", ["r6", "r7"], "set FP for our stack frame");
            EmitInstruction("push", ["r7", "r7"], "cache SP");
            EmitInstruction("push", ["r7", "r5"], "push return address");

            if (scope.Vars.Count > 0)
            {
                EmitInstruction("addi", ["r7", "r7", (-scope.Vars.Count).ToString()], $"create space for {scope.Vars.Count} local variables");
            }
            EmitBlankLine();

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

    private void EmitStackFrameComments(Scope scope)
    {
        var argList = string.Join(", ", scope.Args.Select(a => $"{a.Type} {a.Name}"));
        EmitComment($"function `{scope.Type} {scope.Name}({argList})`");

        foreach (var a in scope.Args)
        {
            EmitComment($" FP+{a.Offset}  {a.Name}");
        }

        EmitComment(" FP+0  previous SP");
        EmitComment(" FP-1  return addr");

        foreach (var v in scope.Vars)
        {
            EmitComment($" FP{v.Offset}  {v.Name}");
        }
        EmitBlankLine();
    }

    private void EmitGlobalVars(Emitter e)
    {
        foreach (var v in e.Cc.GlobalScope.Vars)
        {
            EmitInstruction(label: $"_var_{v.Name}", op: ".fill", operands: ["0"], $"global variable {v.Name}");
        }
    }

    private void EmitInternedStrings(CompilerContext cc)
    {
        foreach (var s in cc.InternedStrings)
        {
            EmitInstruction(label: s.Value, op: ".cstr", operands: [$"\"{Regex.Escape(s.Key)}\""], "interned string");
        }
    }

    private void EmitBlankLine() => Scheduler.BlankLine();

    private void EmitComment(string comment) => Scheduler.InlineComment(comment);

    private void EmitHeaderComment(string comment) => Scheduler.HeaderComment(comment);

    private void EmitLabel(string label) => Scheduler.Label(label);

    private void EmitInstruction(string op, string[]? operands = null, string? comment = null) => EmitInstruction(null, op, operands, comment);

    private void EmitInstruction(string? label = null, string? op = null, string[]? operands = null, string? comment = null)
    {
        var (op1, op2, op3) = ExtractOperands(operands);
        Scheduler.Schedule(new ScheduledInstruction
        {
            Labels = label is not null ? [label] : [],
            Opcode = op is null ? InstrOpcode.Unknown : ToEnum(op),
            Operand1 = op1,
            Operand2 = op2,
            Operand3 = op3,
            Comment = comment,
        });
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
        return $"{root}_{n:d2}";
    }

    private static InstrOpcode ToEnum(string opcode)
    {
        var str = opcode.Replace('.', '_');
        if (Enum.TryParse<InstrOpcode>(str, ignoreCase: true, out var result))
        {
            return result;
        }
        else
        {
            throw new InvalidOperationException($"InstrOpcode ToEnum failed on input \"{opcode}\"");
        }
    }

    private static (string? op1, string? op2, string? op3) ExtractOperands(string[]? operands)
    {
        if (operands is null)
        {
            return (null, null, null);
        }

        return operands.Length switch
        {
            1 => (operands[0], null, null),
            2 => (operands[0], operands[1], null),
            3 => (operands[0], operands[1], operands[2]),
            _ => (null, null, null)
        };
    }
}