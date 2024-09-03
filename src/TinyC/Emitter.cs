using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AnnaSim.TinyC.Antlr;
using AnnaSim.Extensions;

namespace AnnaSim.TinyC;

public partial class Emitter : AnnaCcBaseVisitor<bool>
{
    public string StackTop { get; set; } = "0x8000";

    public CompilerContext Cc { get; internal set; } = new();

    private readonly Dictionary<string, int> labels = [];

    public Emitter(CompilerContext cc) : base()
    {
        Cc = cc;
    }

    public static void Emit(CompilerContext cc, ParserRuleContext context)
    {
        var e = new Emitter(cc);

        EmitProlog();

        e.Visit(context);

        EmitInstruction(".halt", [], "end program");
        EmitBlankLine();

        EmitHeaderComment("start of functions");
        EmitBlankLine();

        EmitFunctionBodies(cc, e);
        EmitBlankLine();

        EmitHeaderComment(".data segment");
        EmitGlobalVars(e);

        EmitInternedStrings(cc);
        EmitBlankLine();

        EmitInstruction("_stack", ".org", [e.StackTop], "stack origination");
    }

    public void RegisterBuiltins()
    {
        Cc.Functions["out"] = new Scope { Name = "out", Type = "void" };
    }

    // This skips the EOF token in the input stream
    public override bool VisitEntrypoint([NotNull] AnnaCcParser.EntrypointContext context) => Visit(context.children[0]);

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
        EmitInstruction("beq", ["r0", $"&{Cc.CurrentScope.Name}_exit"]);

        return true;
    }

    // public override bool VisitFunc_signature([NotNull] AnnaCcParser.Func_signatureContext context)
    // {
    //     if (Cc.Functions.TryGetValue(context.name.Text, out var scope))
    //     {
    //         Cc.CurrentScope = scope;
    //     }
    //     else
    //     {
    //         var funcScope = new Scope { Name = context.name.Text, Type = context.type().GetText() };
    //         Cc.CurrentScope = funcScope;

    //         var funcName = context.name.Text;
    //         var paramList = context.param_list()._param;

    //         paramList.EachReverse((p, i) =>
    //         {
    //             funcScope.Args.Add(new Scope.Var(p.name.Text, p.t.GetText(), i));
    //         });

    //         Cc.Functions[funcScope.Name] = funcScope;
    //     }

    //     return true;
    // }

    // public override bool VisitFunc_proto([NotNull] AnnaCcParser.Func_protoContext context)
    // {
    //     VisitFunc_signature(context.func_signature());
    //     Cc.CurrentScope = Cc.GlobalScope;

    //     return true;
    // }

    // public override bool VisitFunc_decl([NotNull] AnnaCcParser.Func_declContext context)
    // {
    //     VisitFunc_signature(context.func_signature());
    //     functionBodies[context.func_signature().name.Text] = context.block();
    //     Cc.CurrentScope = Cc.GlobalScope;

    //     return true;
    // }

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
            EmitInstruction("pop", ["r7", "r3"]);
            EmitInstruction("out", ["r3"]);
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
            EmitInstruction("push", ["r7", "r4"], $"push {funcName}() return value");
        }

        EmitBlankLine();

        return true;
    }

    // public override bool VisitParam_list([NotNull] AnnaCcParser.Param_listContext context)
    // {
    //     inParamsDecl = true;
    //     foreach (var pc in context._param)
    //     {
    //         VisitSimple_decl(pc);
    //     }
    //     inParamsDecl = false;
    //     return true;
    // }

    // public override bool VisitSimple_decl([NotNull] AnnaCcParser.Simple_declContext context)
    // {
    //     if (inParamsDecl)
    //     {
    //         Cc.CurrentScope.Args.Add(new Scope.Var(context.name.Text, context.type().GetText(), Cc.CurrentScope.Args.Count));
    //     }
    //     else
    //     {
    //         var name = context.name.Text;
    //         var type = context.type().GetText();
    //         Cc.CurrentScope.AddVar(name, type);
    //     }

    //     return true;
    // }

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
                _ => throw new InvalidOperationException($"invalid operator {context.op.Text}")
            };

            if (op is "*" or "/")
            {
                throw new InvalidOperationException("AnnaCC does not support * or /");
            }
            else if (op.StartsWith('b'))
            {
                var label = GetNextLabel("_expr_true");
                EmitInstruction("pop", ["r7", "r2"], $"pop arg2 for op \"{op}\"");
                EmitInstruction("pop", ["r7", "r1"], $"pop arg2 for op \"{op}\"");
                EmitInstruction("sub", ["r3", "r1", "r2"], "compare r1 and r2");
                EmitInstruction(instr, ["r3", $"&{label}"], $"branch if \"{op}\" is true");
                EmitInstruction("lli", ["r3", "0"], "result is 0 otherwise");
                EmitLabel(label);
            }
            else if (Regex.IsMatch(instr, "^[a-z]+$"))
            {
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
                var value = context.INT().GetText();
                EmitInstruction("lwi", ["r3", value], $"load constant {value} -> r3");
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