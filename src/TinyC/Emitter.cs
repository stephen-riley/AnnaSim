using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AnnaSim.TinyC.Antlr;
using AnnaSim.TinyC.Scheduler;
using AnnaSim.Cpu;

namespace AnnaSim.TinyC;

public partial class Emitter : AnnaCcBaseVisitor<bool>
{
    public string StackTop { get; set; } = "0x8000";

    public InstructionScheduler Scheduler { get; } = new();

    public CompilerContext Cc { get; internal set; } = new();

    private readonly Dictionary<string, int> labels = [];

    public Emitter(CompilerContext cc) : base()
    {
        Cc = cc;
    }

    public static InstructionScheduler Emit(CompilerContext cc, ParserRuleContext context, string filename = "(no filename)")
    {
        var e = new Emitter(cc);

        e.EmitProlog(filename);

        e.Visit(context);

        e.EmitInstruction(".halt", [], "end program");
        e.EmitBlankLine();

        e.EmitHeaderComment("start of functions");
        e.EmitBlankLine();

        e.EmitFunctionBodies(cc, e);
        e.EmitBlankLine();

        e.EmitHeaderComment(".data segment");
        e.EmitBlankLine();

        e.EmitGlobalVars(e);
        e.EmitBlankLine();

        e.EmitInternedStrings(cc);
        e.EmitBlankLine();

        e.EmitInstruction("_stack", ".def", [e.StackTop], "stack origination");

        return e.Scheduler;
    }

    public void RegisterBuiltins()
    {
        Cc.Functions["out"] = new Scope { Name = "out", Type = "void" };
    }

    // This skips the EOF token in the input stream
    public override bool VisitEntrypoint([NotNull] AnnaCcParser.EntrypointContext context) => Visit(context.children[0]);

    public override bool VisitIf_stat([NotNull] AnnaCcParser.If_statContext context)
    {
        var start = GetNextLabel("ifst");
        var exit = GetNextLabel("ifend");
        var next = GetNextLabel("ifx");
        var degenerateIf = context.elseblock is null && context._elseifx.Count == 0;

        EmitLabel(start);

        // handle if block
        EmitComment($"{start} test condition");
        VisitExpr(context.ifx);
        EmitInstruction("pop", ["r7", "r3"]);
        if (degenerateIf)
        {
            EmitInstruction("beq", ["r3", "&" + exit], "condition failed, exit");
        }
        else
        {
            EmitInstruction("beq", ["r3", "&" + next], "condition failed, goto next condition");
        }

        EmitBlankLine();
        EmitComment($"{start} block");
        VisitBlock(context.ifblock);
        EmitInstruction("beq", ["r0", "&" + exit], "exit if");
        EmitBlankLine();

        // handle else ifs
        if (context._elseifx.Count > 0)
        {
            for (var i = 0; i < context._elseifx.Count; i++)
            {
                var oldNextLabel = next;
                next = GetNextLabel("ifx");
                EmitComment($"{oldNextLabel} elseif condition");
                EmitLabel(oldNextLabel);
                VisitExpr(context._elseifx[i]);
                EmitInstruction("pop", ["r7", "r3"]);
                EmitInstruction("beq", ["r3", "&" + next], "condition failed, goto next condition");
                EmitComment($"{oldNextLabel} block");
                VisitBlock(context._elseifblock[i]);
                EmitInstruction("beq", ["r0", "&" + exit], "exit if");
                EmitBlankLine();
            }
        }

        if (!degenerateIf)
        {
            EmitLabel(next);
        }

        // handle else block
        if (context.elseblock is not null)
        {
            VisitBlock(context.elseblock);
        }

        EmitLabel(exit);

        return true;
    }

    public override bool VisitVar_decl([NotNull] AnnaCcParser.Var_declContext context)
    {
        VisitSimple_decl(context.simple_decl());

        var name = context.simple_decl().name.Text;

        if (context.e is not null)
        {
            VisitExpr(context.e);
            EmitInstruction("pop", ["r7", "r3"], "load value from stack");

            foreach (var (op, operands, comment) in Cc.CurrentScope.GetStoreIntructions(name))
            {
                EmitInstruction(op, operands, comment);
            }

            EmitBlankLine();
        }

        return true;
    }

    public override bool VisitReturn_stat([NotNull] AnnaCcParser.Return_statContext context)
    {
        VisitExpr(context.expr());
        EmitInstruction("beq", ["r0", $"&{Cc.CurrentScope.Name}_exit"], "return (jump to func exit)");

        return true;
    }

    public override bool VisitFunc_call([NotNull] AnnaCcParser.Func_callContext context)
    {
        // save r3 (push onto stack)
        // NO push args (args successively loaded into r3)
        // load target into r3
        // jalr r3 r5
        // NO pop r3

        var funcName = context.name.Text;
        var args = context.args._args;

        if (funcName == "out")
        {
            VisitExpr(args[0]);
            EmitInstruction("pop", ["r7", "r3"], "pop value for output");
            EmitInstruction("out", ["r3"], "output r3");
            EmitBlankLine();
            return true;
        }

        foreach (var a in args.Reverse())
        {
            VisitExpr(a);
        }

        EmitInstruction("lwi", ["r3", $"&{funcName}"], $"load address of \"{funcName}\" -> r3");
        EmitInstruction("jalr", ["r3", "r5"], $"call function \"{context.name.Text}\"");

        if (Cc.Functions[funcName].Type != "void")
        {
            EmitInstruction("push", ["r7", "r4"], $"push {funcName}(...)'s result");
        }

        EmitBlankLine();

        return true;
    }

    public override bool VisitFunc_decl([NotNull] AnnaCcParser.Func_declContext context)
    {
        // all the functino info was gathered by the semantic analyzer
        return true;
    }

    public override bool VisitExpr([NotNull] AnnaCcParser.ExprContext context)
    {
        if (context.inner is not null)
        {
            VisitExpr(context.inner);
        }
        else if (context.a is not null)
        {
            VisitAtom(context.a);
        }
        else
        {
            // it's a binary expression
            VisitExpr(context.lh);
            VisitExpr(context.rh);
            var op = context.op.Text;
            var instr = op switch
            {
                "+" => "add",
                "-" => "sub",
                "*" => "mul",
                "/" => "div",
                "&&" => "add",
                "||" => "or",
                "^" => "not",
                ">=" => "bge",
                "<=" => "ble",
                ">" => "bgt",
                "<" => "blt",
                "==" => "beq",
                "!=" => "bne",
                _ => throw new InvalidOperationException($"invalid operator {context.op.Text}")
            };

            if (op is "*" or "/")
            {
                throw new InvalidOperationException("AnnaCC does not support * or /");
            }
            else if (instr.StartsWith('b'))
            {
                // var label = GetNextLabel("_true");
                EmitInstruction("pop", ["r7", "r2"], $"pop arg2 for op \"{op}\"");
                EmitInstruction("pop", ["r7", "r1"], $"pop arg1 for op \"{op}\"");
                EmitInstruction("sub", ["r3", "r1", "r2"], "compare r1 and r2");
                // EmitInstruction(instr, ["r3", $"&{label}"], $"branch if \"{op}\" is true");
                EmitInstruction(instr, ["r3", "1"], $"jump past the next instruction if \"{op}\" is true");
                EmitInstruction("lli", ["r3", "0"], "result is false (0) otherwise");
            }
            else if (Regex.IsMatch(instr, "^[a-z]+$"))
            {
                // TODO: fold constants if both sides are constant
                // TODO: simplify operations (no stack) if one side is constant
                EmitInstruction("pop", ["r7", "r2"], $"pop arg2 for op \"{op}\"");
                EmitInstruction("pop", ["r7", "r1"], $"pop arg2 for op \"{op}\"");
                EmitInstruction(instr, ["r3", "r1", "r2"], $"perform \"{op}\" on r1, r2");
            }

            EmitInstruction("push", ["r7", "r3"], $"push value of \"{context.GetText()}\"");
            EmitBlankLine();
        }

        return true;
    }

    public override bool VisitAtom([NotNull] AnnaCcParser.AtomContext context)
    {
        if (context.func_call() is not null)
        {
            VisitFunc_call(context.func_call());
        }
        else
        {
            if (context.INT() is not null)
            {
                var strValue = context.INT().GetText();
                var value = (int)AnnaMachine.ParseMachineInputs([strValue]).First();
                if (value is >= -128 and <= 127)
                {
                    // save an instruction if it's an 8-bit value
                    EmitInstruction("lli", ["r3", strValue], $"load constant {strValue} -> r3");
                }
                else
                {
                    EmitInstruction("lwi", ["r3", strValue], $"load constant {strValue} -> r3");
                }
            }
            else if (context.ID() is not null)
            {
                var id = context.ID().GetText();

                if (Cc.CurrentScope.TryGetByName(id, out var scopeVar))
                {
                    foreach (var (op, operands, comment) in Cc.CurrentScope.GetLoadIntructions(scopeVar.Name))
                    {
                        EmitInstruction(op, operands, comment);
                    }
                }
                else
                {
                    throw new InvalidOperationException($"can't find variable {id} in {Cc.CurrentScope}");
                }
            }
            else if (context.STRING() is not null)
            {
                var strLabel = GetInternedStringLabel(context.STRING().GetText()[1..^1]);
                EmitInstruction("lw", ["r3", $"&{strLabel}"]);
            }
            else if (context.func_call() is not null)
            {
                VisitFunc_call(context.func_call());
            }

            EmitInstruction("push", ["r7", "r3"], "push atom result");
            EmitBlankLine();
        }

        return true;
    }
}