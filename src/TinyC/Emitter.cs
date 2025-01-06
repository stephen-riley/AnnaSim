using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AnnaSim.TinyC.Antlr;
using AnnaSim.TinyC.Scheduler;
using AnnaSim.Cpu;
using System.Runtime.CompilerServices;

namespace AnnaSim.TinyC;

public partial class Emitter : AnnaCcBaseVisitor<bool>
{
    public string StackTop { get; set; } = "0x8000";

    public InstructionScheduler Scheduler { get; } = new();

    public CompilerContext Cc { get; internal set; } = new();

    private readonly Dictionary<string, int> labels = [];

    private static string CurrentMethodName([CallerMemberName] string methodName = null!) => methodName;

    public Emitter(CompilerContext cc) : base()
    {
        Cc = cc;
    }

    public static InstructionScheduler Emit(CompilerContext cc, ParserRuleContext context, string filename)
    {
        var e = new Emitter(cc);

        e.EmitProlog(filename);

        e.Visit(context);

        e.EmitInstruction("halt", [], "end program");
        e.EmitBlankLine();

        if (e.Cc.Functions.Count > 0)
        {
            e.EmitHeaderComment("start of functions");

            e.EmitFunctionBodies(cc, e);
            e.EmitBlankLine();
        }

        e.EmitHeaderComment(".data segment");
        e.EmitBlankLine();

        if (e.Cc.GlobalScope.Vars.Count > 0)
        {
            e.EmitGlobalVars(e);
            e.EmitBlankLine();
        }

        if (cc.InternedStrings.Count > 0)
        {
            e.EmitInternedStrings(cc);
            e.EmitBlankLine();
        }

        e.EmitInstruction("_stack", ".def", [e.StackTop], "stack origination");

        return e.Scheduler;
    }

    public void RegisterBuiltins()
    {
        Cc.Functions["out"] = new Scope { Name = "out", Type = "void" };
    }

    internal void EmitLoadVariable(string id)
    {
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

    internal void EmitStoreVariable(string id, string targetRegister = "r1")
    {
        if (Cc.CurrentScope.TryGetByName(id, out var scopeVar))
        {
            foreach (var (op, operands, comment) in Cc.CurrentScope.GetStoreIntructions(scopeVar.Name, targetRegister))
            {
                EmitInstruction(op, operands, comment);
            }
        }
        else
        {
            throw new InvalidOperationException($"can't find variable {id} in {Cc.CurrentScope}");
        }
    }

    internal void EmitLoadVariableAddress(string id, string targetRegister = "r1")
    {
        if (Cc.CurrentScope.TryGetByName(id, out var scopeVar))
        {
            var (op, operands, _) = Cc.CurrentScope.GetLoadIntructions(scopeVar.Name, targetRegister).First();
            EmitInstruction(op, operands, $"load addr of variable {id} for lval");
        }
        else
        {
            throw new InvalidOperationException($"can't find variable {id} in {Cc.CurrentScope}");
        }
    }

    // This skips the EOF token in the input stream
    public override bool VisitEntrypoint([NotNull] AnnaCcParser.EntrypointContext context) => Visit(context.children[0]);

    public override bool VisitStat([NotNull] AnnaCcParser.StatContext context)
    {
        EmitHeaderComment($".line {context.Start.Line}: {context.GetText()}");
        return base.VisitStat(context);
    }

    public override bool VisitFor_stat([NotNull] AnnaCcParser.For_statContext context)
    {
        var condLabel = GetNextLabel("fcon");
        var exitLabel = GetNextLabel("forx");

        if (context.init is not null)
        {
            EmitComment("for loop init");
            VisitVar_decl(context.init);
        }
        else
        {
            EmitComment("for loop with no init");
        }

        EmitComment("for loop condition");
        EmitLabel(condLabel);
        VisitExpr(context.cond);
        EmitInstruction("pop", ["rSP", "r3"]);
        EmitInstruction("beq", ["r3", "&" + exitLabel], "exit on false");
        EmitBlankLine();

        EmitComment("block");
        VisitBlock(context.block());
        EmitBlankLine();

        if (context.update is not null)
        {
            EmitComment("for loop update");
            VisitAssign(context.update);
        }
        else
        {
            EmitComment("no for loop update given");
        }

        EmitInstruction("br", ["&" + condLabel]);
        EmitBlankLine();

        EmitComment("exit for loop");
        EmitLabel(exitLabel);

        return true;
    }

    public override bool VisitWhile_stat([NotNull] AnnaCcParser.While_statContext context)
    {
        var condLabel = GetNextLabel("whcon");
        var blockLabel = GetNextLabel("whb");
        var exitLabel = GetNextLabel("whx");

        EmitComment("begin while loop condition");
        EmitLabel(condLabel);
        VisitExpr(context.expr());
        EmitInstruction("pop", ["rSP", "r3"]);
        EmitInstruction("beq", ["r3", "&" + exitLabel], "exit on false");
        EmitBlankLine();

        EmitComment("block");
        EmitLabel(blockLabel);
        VisitBlock(context.block());

        // EmitInstruction("beq", ["r0", "&" + condLabel]);
        EmitInstruction("br", ["&" + condLabel]);
        EmitBlankLine();

        EmitComment("exit while loop");
        EmitLabel(exitLabel);

        return true;
    }

    public override bool VisitDo_while_stat([NotNull] AnnaCcParser.Do_while_statContext context)
    {
        var condLabel = GetNextLabel("whcon");
        var blockLabel = GetNextLabel("whb");
        var exitLabel = GetNextLabel("whx");

        EmitComment("begin do-while loop");
        EmitInstruction("beq", ["r0", "&" + blockLabel], "jump to body");
        EmitBlankLine();

        EmitComment("do-while loop condition");
        EmitLabel(condLabel);
        VisitExpr(context.expr());
        EmitInstruction("pop", ["rSP", "r3"]);
        EmitInstruction("beq", ["r3", "&" + exitLabel], "exit on false");
        EmitBlankLine();

        EmitComment("block");
        EmitLabel(blockLabel);
        VisitBlock(context.block());

        EmitInstruction("beq", ["r0", "&" + condLabel]);
        EmitBlankLine();

        EmitComment("exit do-while loop");
        EmitLabel(exitLabel);

        return true;
    }

    public override bool VisitIf_stat([NotNull] AnnaCcParser.If_statContext context)
    {
        var start = GetNextLabel("ifs");
        var block = GetNextLabel("ifb");
        var exit = GetNextLabel("ifxx");
        var next = GetNextLabel("ifx");
        var degenerateIf = context.elseblock is null && context._elseifx.Count == 0;

        EmitLabel(start);

        // handle if block
        EmitComment($"{start} test condition");
        VisitExpr(context.ifx);
        EmitInstruction("pop", ["rSP", "r2"]);

        if (degenerateIf)
        {
            EmitInstruction("beq", ["r2", "&" + exit], "condition failed, exit");
        }
        else
        {
            EmitInstruction("beq", ["r2", "&" + next], "condition failed, goto next condition");
        }

        EmitBlankLine();
        EmitComment($"{start} block");
        EmitLabel(block);
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
                EmitInstruction("pop", ["rSP", "r3"]);
                EmitInstruction("beq", ["r3", "&" + next], "condition failed, goto next condition");
                EmitComment($"{oldNextLabel} block");
                EmitLabel(GetNextLabel("ifb"));
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
            EmitComment($"{start} else block");
            EmitLabel(GetNextLabel("ifb"));
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
            EmitInstruction("pop", ["rSP", "r3"], "load value from stack");

            EmitStoreVariable(name);

            EmitBlankLine();
        }

        return true;
    }

    public override bool VisitReturn_stat([NotNull] AnnaCcParser.Return_statContext context)
    {
        VisitExpr(context.expr());
        EmitInstruction("pop", ["rSP", "r4"], "load function result -> r4");
        EmitInstruction("beq", ["r0", $"&{Cc.CurrentScope.Name}_exit"], "return (jump to func exit)");

        return true;
    }

    private bool HandleBuiltins(AnnaCcParser.Func_callContext context)
    {
        var funcName = context.name.Text;
        var args = context.args._args;

        switch (funcName)
        {
            case "in":
                EmitInstruction("in", ["r3"]);
                EmitInstruction("push", ["rSP", "r3"], "push input onto stack");
                return true;

            case "out":
                VisitExpr(args[0]);
                EmitInstruction("pop", ["rSP", "r3"], "pop value for output");
                EmitInstruction("out", ["r3"], "output r3");
                EmitBlankLine();
                return true;

            case "print":
                VisitExpr(args[0]);
                EmitInstruction("pop", ["rSP", "r3"], "pop value for output");
                EmitInstruction("outs", ["r3"], "print string at r3");
                EmitBlankLine();
                return true;

            case "printn":
                VisitExpr(args[0]);
                EmitInstruction("pop", ["rSP", "r3"], "pop value for output");
                EmitInstruction("outn", ["r3"], "print int at r3");
                EmitBlankLine();
                return true;

            case "println":
                Cc.InternedStrings["\n"] = "__nl";
                EmitInstruction("lwi", ["r1", "&__nl"], "load addr of newline");
                EmitInstruction("outs", ["r1"], "print newline");
                EmitBlankLine();
                return true;

            default:
                return false;
        }
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

        if (HandleBuiltins(context))
        {
            return true;
        }

        foreach (var a in args.Reverse())
        {
            VisitExpr(a);
        }

        EmitInstruction("lwi", ["r1", $"&{funcName}"], $"load address of \"{funcName}\" -> r1");
        EmitInstruction("jalr", ["r1", "r5"], $"call function \"{context.name.Text}\"");

        if (Cc.Functions[funcName].Type != "void")
        {
            EmitInstruction("push", ["rSP", "r4"], $"push {funcName}(...)'s result");
        }

        EmitBlankLine();

        return true;
    }

    public override bool VisitFunc_decl([NotNull] AnnaCcParser.Func_declContext context)
    {
        // all the functino info was gathered by the semantic analyzer
        return true;
    }

    public override bool VisitAssign([NotNull] AnnaCcParser.AssignContext context)
    {
        if (context.ID() is not null)
        {
            var id = context.ID().GetText();

            if (context.op is not null)
            {
                EmitLoadVariable(id);
                var val = context.op.Text switch
                {
                    "++" => "1",
                    "--" => "-1",
                    _ => throw new InvalidOperationException($"unknown postfix operator ${context.op.Text}")
                };
                var desc = context.op.Text == "++" ? "increment" : "decrement";
                EmitInstruction("addi", ["r3", "r3", val], $"{desc} {id}");
                EmitStoreVariable(id);
            }
            else if (context.opeq is not null)
            {
                var opcode = context.opeq.Text switch
                {
                    "+=" => "add",
                    "-=" => "sub",
                    "*=" => "mul",
                    "/=" => "div",
                    _ => throw new InvalidOperationException($"unknown operator {context.opeq.Text}")
                };

                VisitExpr(context.opexpr);
                EmitInstruction("pop", ["rSP", "r2"], $"set up r2 as rhs of {context.opeq.Text}");

                EmitLoadVariable(id);
                EmitInstruction(opcode, ["r3", "r3", "r2"], $"execute {context.opeq.Text}");
                EmitStoreVariable(id);
            }
        }
        else
        {
            // We'll use r2 to store the lhs and r3 the rhs.  r2 will have the address
            //  at which to store the value in r3.
            VisitLexpr(context.lval);
            VisitExpr(context.rhs);

            EmitInstruction("pop", ["rSP", "r3"], "load lexpr rh");
            EmitInstruction("pop", ["rSP", "r2"], "load lexpr lh");

            EmitInstruction("sw", ["r3", "r2", "0"], "assign r3 to lval r2");
            EmitBlankLine();
        }

        return true;
    }

    public override bool VisitExpr([NotNull] AnnaCcParser.ExprContext context)
    {
        EmitComment(Cc.TracingComments ? $"ENTERING {CurrentMethodName()}: «{context.GetText()}»" : "");

        if (context.unary is not null)
        {
            // array deref
            if (context.index is not null)
            {
                // get the base address, push it onto the stack
                VisitExpr(context.unary);
                // calculate the offset, push it onto the stack
                VisitExpr(context.index);
                EmitInstruction("pop", ["rSP", "r2"], "load array offset into r2");
                EmitInstruction("pop", ["rSP", "r3"], "load array base into r3");
                EmitInstruction("add", ["r3", "r2", "r3"], "calculate address of array offset");
                EmitInstruction("lw", ["r3", "r3", "0"], "load contents of array at offset");
                EmitInstruction("push", ["rSP", "r3"], "push contents");
            }
            // deref operator
            else if (context.op.Text == "*")
            {
                VisitExpr(context.unary);
                EmitInstruction("pop", ["rSP", "r3"], "loading address to deref");
                EmitInstruction("lw", ["r3", "r3", "0"], "deref r3");
                EmitInstruction("push", ["rSP", "r3"], "store derefed value");
            }
            // optimization for int literals
            else if (context.unary.Start.Type == AnnaCcLexer.INT)
            {
                var strValue = context.unary.GetText();
                var intValue = (int)AnnaMachine.ParseMachineInputs([strValue]).First();
                var signedValue = context.op.Text == "-" ? -intValue : intValue;
                var opcode = (signedValue is >= -128 and <= 127) ? "lli" : "lwi";
                if (context.op.Text == "-")
                {
                    EmitInstruction(opcode, ["r3", signedValue.ToString()], $"load constant r3={signedValue}");
                }
                EmitInstruction(opcode, ["r3", strValue], $"load constant r3={strValue}");
                EmitInstruction("push", ["rSP", "r3"]);
            }
            // unary +/- on expression
            else
            {
                VisitExpr(context.unary);
                if (context.op.Text == "+")
                {
                    EmitComment($"redundant unary + ignored in expression {context.GetText()}");
                }
                else if (context.op.Text == "-")
                {
                    EmitInstruction("pop", ["rSP", "r3"], "pop value to invert it");
                    EmitInstruction("sub", ["r3", "r0", "r3"], "invert r3");
                    EmitInstruction("push", ["rSP", "r3"], "push inverted value onto stack");
                }
                else
                {
                    throw new InvalidOperationException($"unknown unary operator {context.op.Text}");
                }
            }
        }
        else if (context.inner is not null)
        {
            VisitExpr(context.inner);
        }
        else if (context.a is not null)
        {
            VisitAtom(context.a);
        }
        else if (context.arryderef is not null)
        {
            VisitLexpr(context.arryderef);
            VisitExpr(context.index);
            EmitInstruction("pop", ["rSP", "r3"], "load offset for array access");
            EmitInstruction("pop", ["rSP", "r2"], "load base addr for array access");
            EmitInstruction("add", ["r3", "r2", "r3"], "calculate address of element");
            EmitInstruction("lw", ["r3", "r3", "0"], "load array element");
            EmitInstruction("push", ["rSP", "r3"], "push element");
        }
        else
        {
            // It's a binary expression.  By the end of this, r3 contians the lhs,
            //  and r2 contains the rhs.

            // If the rhs is just a constant int, then load it into r2
            bool constRhs = false;
            if (context.rh.Start.Type == AnnaCcLexer.INT)
            {
                // EmitComment("OPTMIZATION: don't push rhs constant onto stack");
                EmitInstruction("lwi", ["r2", context.rh.GetText()], "directly load rhs constant");
                constRhs = true;
            }
            // If the rhs is just a constant int, then load it into r2
            else if (context.rh?.a?.sz is not null)
            {
                // EmitComment("OPTMIZATION: don't push rhs constant onto stack");
                EmitSizeofAtom(context.rh.a.sz, "r2");
                constRhs = true;
            }
            else if (context.rh is not null)
            {
                VisitExpr(context.rh);
            }
            else
            {
                throw new InvalidOperationException("unknown alternative for rule expr");
            }

            VisitExpr(context.lh);

            var op = context.op.Text;
            var instr = op switch
            {
                "+" => "add",
                "-" => "sub",
                "*" => "mul",
                "/" => "div",
                "%" => "mod",
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

            if (instr.StartsWith('b'))
            {
                if (!constRhs)
                {
                    EmitInstruction("pop", ["rSP", "r2"], $"pop arg2 for op \"{op}\"");
                }
                EmitInstruction("pop", ["rSP", "r3"], $"pop arg1 for op \"{op}\"");
                EmitInstruction("sub", ["r2", "r3", "r2"], "compare r2 and r3");
                EmitInstruction("lli", ["r3", "1"], "assume true preemptively");
                EmitInstruction(instr, ["r2", "1"], $"jump past the next instruction if \"{op}\" is true");
                EmitInstruction("lli", ["r3", "0"], "result is false (0) otherwise");
            }
            else if (Regex.IsMatch(instr, "^[a-z]+$"))
            {
                // TODO: fold constants if both sides are constant
                if (!constRhs)
                {
                    EmitInstruction("pop", ["rSP", "r2"], $"pop arg2 for op \"{op}\"");
                }
                EmitInstruction("pop", ["rSP", "r3"], $"pop arg1 (lhs) for op \"{op}\"");
                EmitInstruction(instr, ["r3", "r3", "r2"], $"perform \"{op}\" on r2, r3");
            }
            else
            {
                EmitComment($"why are we here? {nameof(VisitExpr)}");
            }

            EmitInstruction("push", ["rSP", "r3"], $"push result of \"{context.GetText()}\"");
            EmitBlankLine();
        }

        return true;
    }

    public override bool VisitSizeof_atom([NotNull] AnnaCcParser.Sizeof_atomContext context)
    {
        EmitSizeofAtom(context);
        EmitInstruction("push", ["rSP", "r3"], $"push {context.GetText()}");
        return true;
    }

    private bool EmitSizeofAtom([NotNull] AnnaCcParser.Sizeof_atomContext context, string destRegister = "r3")
    {
        if (context.id is not null)
        {
            if (Cc.CurrentScope.TryGetByName(context.id.Text, out var scopeVar))
            {
                var size = 1;
                if (scopeVar.Type.Contains('['))
                {
                    size = int.Parse(scopeVar.Type.Split('[', ']')[1]);
                }

                EmitInstruction("lli", [destRegister, size.ToString()], $"load {context.GetText()}");
            }
            else
            {
                throw new InvalidOperationException($"can't find variable {context.id.Text}");
            }
        }
        else if (context.t is not null)
        {
            EmitInstruction("lli", [destRegister, "1"], $"load {context.GetText()}");
        }
        else
        {
            throw new InvalidOperationException($"don't know how to get {context.GetText()}");
        }
        return true;
    }

    public override bool VisitAtom([NotNull] AnnaCcParser.AtomContext context)
    {
        EmitComment(Cc.TracingComments ? $"ENTERING {CurrentMethodName()}: «{context.GetText()}»" : "");

        if (context.sz is not null)
        {
            VisitSizeof_atom(context.sz);
        }
        else if (context.func_call() is not null)
        {
            VisitFunc_call(context.func_call());
        }
        else if (context.INT() is not null)
        {
            var strValue = context.INT().GetText();
            var value = (int)AnnaMachine.ParseMachineInputs([strValue]).First();
            if (value is >= -128 and <= 127)
            {
                // save an instruction if it's an 8-bit value
                EmitInstruction("lli", ["r3", strValue], $"load constant r3={strValue}");
            }
            else
            {
                EmitInstruction("lwi", ["r3", strValue], $"load constant r3={strValue}");
            }
            EmitInstruction("push", ["rSP", "r3"], "push result");
        }
        else if (context.CHAR() is not null)
        {
            var strValue = context.CHAR().GetText();
            var value = (byte)strValue[1];
            EmitInstruction("lli", ["r3", $"{value}"], $"load constant '{strValue[1]}'");
            EmitInstruction("push", ["rSP", "r3"], "push result");
        }
        else if (context.ID() is not null)
        {
            var id = context.ID().GetText();

            EmitLoadVariable(id);
            EmitInstruction("push", ["rSP", "r3"], "push result");
        }
        else if (context.STRING() is not null)
        {
            var strLabel = GetInternedStringLabel(context.STRING().GetText()[1..^1]);
            EmitInstruction("lwi", ["r3", $"&{strLabel}"]);
            EmitInstruction("push", ["rSP", "r3"], "push result");
        }
        else
        {
            EmitComment($"why are we here? {nameof(VisitAtom)}");
        }

        // EmitInstruction("push", ["rSP", "r3"], "push result");
        EmitBlankLine();

        return true;
    }

    public override bool VisitLexpr([NotNull] AnnaCcParser.LexprContext context)
    {
        EmitComment(Cc.TracingComments ? $"ENTERING {CurrentMethodName()}: «{context.GetText()}»" : "");

        if (context.inner is not null)
        {
            return VisitLexpr(context.inner);
        }
        else if (context.unary is not null)
        {
            var op = context.op.Text;
            VisitLexpr(context.unary);

            if (op == "+")
            {
                EmitComment($"redundant unary + ignored in expression {context.GetText()} [optimizer can remove]");
            }
            else if (op == "-")
            {
                EmitInstruction("pop", ["rSP", "r3"], "pop value to invert it");
                EmitInstruction("sub", ["r3", "r0", "r3"], "invert r3");
                EmitInstruction("push", ["rSP", "r3"], "push inverted value onto stack");
            }
            else if (op == "*")
            {
                EmitInstruction("pop", ["rSP", "r3"], "pop value to deref it");
                EmitInstruction("lw", ["r3", "r3", "0"], "deref r3");
                EmitInstruction("push", ["rSP", "r3"], "push derefed value onto stack");
            }
            else
            {
                throw new InvalidOperationException($"unknown unary operator {context.op.Text}");
            }

            EmitBlankLine();
        }
        else if (context.a is not null)
        {
            VisitLatom(context.a);
        }
        else if (context.op is not null)
        {
            // We'll use r2 to store the lhs and r3 the rhs.  r2 will have the address
            //  at which to store the value in r3.
            VisitLexpr(context.lh);
            VisitLexpr(context.rh);

            EmitInstruction("pop", ["rSP", "r3"], "load lexpr rh");
            EmitInstruction("pop", ["rSP", "r2"], "load lexpr lh");

            EmitInstruction("sw", ["r3", "r2", "0"], "store r3 to lval");
        }

        return true;
    }

    public override bool VisitLatom([NotNull] AnnaCcParser.LatomContext context)
    {
        EmitComment(Cc.TracingComments ? $"ENTERING {CurrentMethodName()}: «{context.GetText()}»" : "");

        if (context.INT() is not null)
        {
            var strValue = context.INT().GetText();
            var value = (int)AnnaMachine.ParseMachineInputs([strValue]).First();
            if (value is >= -128 and <= 127)
            {
                // save an instruction if it's an 8-bit value
                EmitInstruction("lli", ["r3", strValue], $"load constant r3={strValue}");
            }
            else
            {
                EmitInstruction("lwi", ["r3", strValue], $"load constant r3={strValue}");
            }
        }
        else if (context.ID() is not null)
        {
            var id = context.ID().GetText();

            EmitLoadVariableAddress(id, "r3");
        }
        else if (context.func_call() is not null)
        {
            VisitFunc_call(context.func_call());
        }
        else
        {
            EmitComment($"why are we here? {nameof(VisitLatom)}");
        }

        EmitInstruction("push", ["rSP", "r3"], "push result");
        EmitBlankLine();

        return true;
    }
}