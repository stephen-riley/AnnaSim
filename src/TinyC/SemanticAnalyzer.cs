using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AnnaSim.TinyC.Antlr;

namespace AnnaSim.TinyC;

public class SemanticAnalyzer : AnnaCcBaseVisitor<bool>
{
    public CompilerContext Cc { get; } = new();

    public List<string> Errors { get; } = [];

    public bool HasErrors { get => Errors.Count != 0; }

    private bool inParamsDecl;

    public SemanticAnalyzer() : base()
    {
    }

    public static SemanticAnalyzer Evaluate(IParseTree tree)
    {
        var evaluator = new SemanticAnalyzer();
        evaluator.Visit(tree);

        foreach (var f in evaluator.Cc.Functions.Values)
        {
            if (!f.HasBody)
            {
                throw new InvalidOperationException($"function {f.Name} does not have a body");
            }
        }

        return evaluator;
    }

    // This skips the EOF token in the input stream
    public override bool VisitEntrypoint([NotNull] AnnaCcParser.EntrypointContext context) => Visit(context.children[0]);

    private static string FullSimpleDeclType(AnnaCcParser.Simple_declContext context)
    {
        var baseType = context.type().GetText();
        if (context.arry is not null)
        {
            var size = context.size is not null ? context.size.Text : "";
            baseType += $"[{size}]";
        }
        return baseType;
    }

    public override bool VisitSimple_decl([NotNull] AnnaCcParser.Simple_declContext context)
    {
        var name = context.name.Text;
        var type = context.type().GetText();

        if (inParamsDecl)
        {
            Cc.CurrentScope.AddArg(name, type);
        }
        else
        {
            var declType = FullSimpleDeclType(context);
            Cc.CurrentScope.AddVar(name, declType);
        }

        return true;
    }

    public override bool VisitVar_decl([NotNull] AnnaCcParser.Var_declContext context)
    {
        VisitSimple_decl(context.simple_decl());
        var varName = context.simple_decl().name.Text;

        var scopeVar = Cc.CurrentScope.GetByName(varName) ?? throw new InvalidOperationException($"cannot find variable {varName}");

        var arrayDeclaredSize = -1;

        if (context.simple_decl().size is not null)
        {
            arrayDeclaredSize = int.Parse(context.simple_decl().size.Text);
        }

        if (context.a is not null && context.a.sz is null && context.a.fn is null)
        {
            Cc.CurrentScope.UpdateVar(scopeVar with { DefaultValue = context.a.GetText() });
        }
        if (context.e is not null)
        {
            VisitExpr(context.e);
        }
        else if (context.al is not null)
        {
            VisitArray_literal(context.al);
            Cc.CurrentScope.UpdateVar(scopeVar with { DefaultValue = context.al.GetText() });
            var elementCount = context.al.GetText().Split(',').Length;
            if (arrayDeclaredSize == -1)
            {
                // update the scope var entry with the array literal's size
                var newType = scopeVar.Type.Replace("[]", $"[{elementCount}]");
                Cc.CurrentScope.UpdateVar(scopeVar with { Type = newType, DefaultValue = context.al.GetText() });
            }
            else if (elementCount != arrayDeclaredSize)
            {
                throw new InvalidOperationException($"declared array size {context.GetText()} does not match element count in array literal {context.al.GetText()}");
            }
        }

        return true;
    }

    public override bool VisitParam_list([NotNull] AnnaCcParser.Param_listContext context)
    {
        inParamsDecl = true;
        foreach (var pc in context._param)
        {
            VisitSimple_decl(pc);
        }
        inParamsDecl = false;
        return true;
    }

    public override bool VisitFunc_signature([NotNull] AnnaCcParser.Func_signatureContext context)
    {
        VisitParam_list(context.param_list());

        return true;
    }

    public override bool VisitFunc_proto([NotNull] AnnaCcParser.Func_protoContext context)
    {
        var name = context.func_signature().name.Text;
        var type = context.func_signature().type().GetText();

        if (!Cc.Functions.TryGetValue(name, out Scope? scope))
        {
            scope = new() { Name = name, Type = type };
            Cc.Functions[name] = scope;
        }

        Cc.CurrentScope = scope;
        VisitFunc_signature(context.func_signature());
        Cc.CurrentScope = Cc.GlobalScope;

        return true;
    }

    public override bool VisitFunc_decl([NotNull] AnnaCcParser.Func_declContext context)
    {
        var name = context.func_signature().name.Text;
        var type = context.func_signature().type().GetText();

        bool scopeExisted = false;

        if (Cc.Functions.TryGetValue(name, out var scope))
        {
            scopeExisted = true;
            // if we're here, there was either a prototype already processed,
            //  or it's a redefinition.  Redifinition is bad, matching prototype
            //  (args and type match) is good.

            var scopeType = scope.Type;
            var contextType = context.func_signature().type().GetText();
            var scopeArgs = scope.ArgsString;
            var contextArgs = context.func_signature().param_list().GetText();

            if (scope.HasBody)
            {
                throw new InvalidOperationException($"attempted body redefinition of function \"{name}(...)\"");
            }
            else if (contextType != scopeType)
            {
                throw new InvalidOperationException($"attempted type redefinition of function \"{name}(...)\"");
            }
            else if (contextArgs != scopeArgs)
            {
                throw new InvalidOperationException($"attempted signature redefinition of function \"{name}(...)\"");
            }

            scope.Body = context.body;
        }
        else
        {
            scope = new Scope { Name = name, Type = type };
        }

        Cc.Functions[name] = scope;
        Cc.CurrentScope = scope;

        if (!scopeExisted)
        {
            VisitFunc_signature(context.func_signature());
        }

        VisitBlock(context.body);

        Cc.CurrentScope.Body = context.body;
        Cc.CurrentScope = Cc.GlobalScope;

        return true;
    }

    public override bool VisitFunc_call([NotNull] AnnaCcParser.Func_callContext context)
    {
        // TODO: need to set up a dictionary of builtins that this and the emitter can use
        if (context.name.Text is "in" or "out" or "print" or "printn" or "println")
        {
            return true;
        }

        if (!Cc.Functions.ContainsKey(context.name.Text))
        {
            Errors.Add($"function named {context.name.Text} does not exist (AnnaCC does not support forward references)");
            return false;
        }
        else
        {
            return base.VisitFunc_call(context);
        }
    }
}