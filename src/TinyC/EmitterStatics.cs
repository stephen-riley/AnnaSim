using AnnaSim.AsmParsing;
using AnnaSim.Instructions;
using AnnaSim.TinyC.Antlr;

namespace AnnaSim.TinyC;

public partial class Emitter : AnnaCcBaseVisitor<bool>
{
    private void EmitProlog(string filename)
    {
        EmitHeaderComment($"compiled from {filename}");
        EmitBlankLine();
        EmitHeaderComment("Register map:");
        EmitHeaderComment("  r1  scratch register for addresses");
        EmitHeaderComment("  r2  scratch register for temps");
        EmitHeaderComment("  r3  scratch register for expressions results");
        EmitHeaderComment("  r4  result from function calls");
        EmitHeaderComment("  r5  function call destinations (jalr)");
        EmitHeaderComment("  r6  current frame pointer (FP)");
        EmitHeaderComment("  r7  stack pointer (SP)");

        EmitBlankLine();
        EmitInstruction(".ralias", ["rFP", "r6"], "alias Frame Pointer to r6");
        EmitInstruction(".ralias", ["rSP", "r7"], "alias Stack Pointer to r7");

        EmitBlankLine();
        EmitHeaderComment(".text segment");
        EmitBlankLine();
        EmitInstruction(".org", ["0x0000"]);
        EmitBlankLine();

        EmitComment("set up main() stack frame");
        EmitInstruction("lwi", ["rSP", "&_stack"], "initialize SP (r7)");
        EmitInstruction("mov", ["rFP", "rSP"], "initialize FP (r6)");
        EmitBlankLine();
        EmitHeaderComment("start of main");
    }

    private void EmitFunctionBodies(CompilerContext cc, Emitter e)
    {
        foreach (var (name, scope) in e.Cc.Functions)
        {
            e.Cc.CurrentScope = scope;

            var epExit = $"{name}_exit";

            EmitBlankLine();
            EmitStackFrameComments(scope);

            EmitLabel(name);
            // TODO: put SP and return address onto the stack using sw's,
            //  and then bump SP 2 + #locals
            EmitInstruction("push", ["rSP", "rFP"], "cache FP");
            EmitInstruction("addi", ["rFP", "rSP", "1"], "set up new FP");
            EmitInstruction("push", ["rSP", "r5"], "push return address");

            if (scope.Vars.Count > 0)
            {
                EmitInstruction("addi", ["rSP", "rSP", (-scope.Vars.Count).ToString()], $"create space for stack frame");
            }
            EmitBlankLine();

            EmitInstruction(".frame", ["\"on\""]);
            EmitLabel($"{name}_body");
            e.VisitBlock(scope.Body);

            EmitBlankLine();
            EmitInstruction(".frame", ["\"off\""]);

            // unwind stack and return
            EmitLabel(epExit);
            EmitInstruction("lw", ["r5", "rFP", "-1"], "load return addr from FP-1");
            EmitInstruction("lw", ["rFP", "rFP", "0"], "restore previous FP");
            EmitInstruction("addi", ["rSP", "rSP", scope.FrameSize.ToString()], "collapse stack frame");
            EmitInstruction("jmp", ["r5"], "return from function");
        }
    }

    private void EmitStackFrameComments(Scope scope)
    {
        var argList = string.Join(", ", scope.Args.Values.Select(a => $"{a.Type} {a.Name}"));
        EmitHeaderComment($"function `{scope.Type} {scope.Name}({argList})`");

        var frameDef = scope.Name;
        foreach (var a in scope.Args.Values)
        {
            frameDef += $"|{a.Offset}~{a.Name}";
        }
        frameDef += "|0~prev FP|-1~ret addr";
        foreach (var v in scope.Vars.Values)
        {
            frameDef += $"|{v.Offset}~{v.Name}";
        }

        foreach (var a in scope.Args.Values)
        {
            EmitHeaderComment($" FP+{a.Offset}  {a.Name}");
        }

        EmitHeaderComment(" FP+0  previous FP");
        EmitHeaderComment(" FP-1  return addr");

        foreach (var v in scope.Vars.Values)
        {
            EmitHeaderComment($" FP{v.Offset}  {v.Name}");
        }
        EmitBlankLine();
        EmitInstruction(".frame", ['"' + frameDef + '"']);
    }

    private void EmitGlobalVars(Emitter e)
    {
        foreach (var v in e.Cc.GlobalScope.Vars.Values)
        {
            string[] operands = ["0"];
            if (v.DefaultValue is not null)
            {
                if (v.DefaultValue.Contains('{'))
                {
                    operands = v.DefaultValue.Replace("{", "").Replace("}", "").Replace(" ", "").Split(',');
                }
                else
                {
                    operands = [v.DefaultValue];
                }
            }
            EmitInstruction(label: $"_var_{v.Name}", op: ".fill", operands: operands, $"global variable {v.Name}");
        }
    }

    private void EmitInternedStrings(CompilerContext cc)
    {
        foreach (var s in cc.InternedStrings)
        {
            // We can leave most characters alone, but whitepace chars might mess up the output.
            var escaped = s.Key.Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r");
            EmitInstruction(label: s.Value, op: ".cstr", operands: [$"\"{escaped}\""], "interned string");
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
        Scheduler.Schedule(new CstInstruction
        {
            Labels = label is not null ? [label] : [],
            Opcode = op is null ? InstrOpcode.Unknown : ToEnum(op),
            OperandStrings = operands is not null ? operands : [],
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
        return $"{root}{n:d2}";
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