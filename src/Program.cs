using AnnaSim.Assembler;
using AnnaSim.Cpu;
using AnnaSim.Debugger;
using AnnaSim.TinyC;
using CommandLine;

var cliParser = new Parser(with =>
{
    with.AllowMultiInstance = true;
    with.HelpWriter = null;
});

try
{
    var parserResult = cliParser
        .ParseArguments<AnnaSimContext>(args)
        .WithParsed(ExecutionPipeline);

    parserResult.WithNotParsed(errs => AnnaSimContext.DisplayHelp(parserResult, errs));
}
catch (Exception e)
{
    Console.Error.WriteLine();
    Console.Error.Write(e.Message);
    if (e.InnerException is not null)
    {
        Console.Error.Write($" ({e.InnerException.Message})");
    }
    Console.WriteLine();
}

void ExecutionPipeline(AnnaSimContext opt)
{
    if (opt.InputFilename is not null && opt.InputFilename.EndsWith(".mem"))
    {
        throw new NotImplementedException(".mem files not yet supported");
    }

    opt.Source = ReadInputFile(opt);

    if (opt.Cc || (opt.InputFilename?.EndsWith(".c") ?? false))
    {
        Compile(opt);
    }

    Assemble(opt);

    if (opt.Run)
    {
        Run(opt);
    }
    else if (opt.Debug || opt.AdvancedDebug)
    {
        BaseDebugger debugger = opt.AdvancedDebug
                       ? new global::AnnaSim.Debugger.Vt100ConsoleDebugger(opt.CstProgram, opt.Inputs.ToArray<string>(), opt.DebugCommands.ToArray<string>())
                       : new global::AnnaSim.Debugger.ConsoleDebugger(opt.CstProgram, opt.Inputs.ToArray<string>(), opt.DebugCommands.ToArray<string>());

        debugger.Run(opt.DumpScreen);
    }
    else
    {
        opt.CstProgram.MemoryImage?.WriteMemFile("-");
    }
}

void Compile(AnnaSimContext opt)
{
    if (Compiler.TryCompile(opt.Source, out var asmSource, opt.InputFilename, optimization: opt.OptimizationLevel))
    {
        opt.AsmSource = asmSource;
        opt.Source = asmSource;

        if (opt.SaveAsmFilename is not null)
        {
            File.WriteAllText(opt.SaveAsmFilename, asmSource);
        }
    }
    else
    {
        throw new InvalidOperationException("compile failed");
    }
}

void Assemble(AnnaSimContext opt)
{
    var asm = new AnnaAssembler();
    opt.CstProgram = asm.Assemble(opt.Source);

    if (opt.CstProgram is null)
    {
        throw new NullReferenceException($"{nameof(opt.CstProgram)} is null");
    }

    if (opt.MemoryFilename is not null)
    {
        opt.CstProgram.MemoryImage?.WriteMemFile(opt.MemoryFilename);
    }

    if (opt.DisassemblyFilename is not null)
    {
        using var fw = new StreamWriter(opt.DisassemblyFilename) { AutoFlush = true };
        foreach (var i in opt?.CstProgram?.Instructions ?? [])
        {
            i.Render(fw, true);
        }
    }
}

void Run(AnnaSimContext opt)
{
    var runner = new Runner(opt.CstProgram, opt.Inputs.ToArray(), trace: opt.Trace);
    runner.Run(opt.DumpScreen);
}

string ReadInputFile(AnnaSimContext opt)
{
    if (opt.InputFilename is not null)
    {
        return File.ReadAllText(opt.InputFilename);
    }
    else if (Console.IsInputRedirected)
    {
        var list = new List<string>();
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            list.Add(line);
        }

        return string.Join("\n", list);
    }
    else
    {
        throw new InvalidOperationException("Must specify an input file or redirect STDIN (did you mean to say --help?)");
    }
}
