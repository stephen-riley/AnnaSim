﻿using System.Text;
using AnnaSim.Cli;
using AnnaSim.Debugger;
using AnnaSim.TinyC;
using CommandLine;

var cliParser = new Parser(settings =>
{
    settings.AllowMultiInstance = true;
    settings.HelpWriter = Console.Error;
});

try
{
    cliParser.ParseArguments<RunCliOptions, DebugCliOptions, CompileCliOptions>(args)
        .WithParsed<RunCliOptions>(HandleRun)
        .WithParsed<DebugCliOptions>(HandleDebug)
        .WithParsed<CompileCliOptions>(HandleCompile);
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

void HandleRun(RunCliOptions opt)
{
    var runner = new Runner(opt.Filename, opt.Inputs.ToArray());
    runner.Run(opt.DumpScreen);
}
void HandleDebug(DebugCliOptions opt)
{
    BaseDebugger debugger = opt.Vt100
                   ? new Vt100ConsoleDebugger(opt.Filename, opt.Inputs.ToArray(), opt.DebugCommands.ToArray())
                   : new ConsoleDebugger(opt.Filename, opt.Inputs.ToArray(), opt.DebugCommands.ToArray());

    debugger.Run(opt.DumpScreen);
}

void HandleCompile(CompileCliOptions opt)
{
    string src;
    if (opt.Filename != "-")
    {
        src = File.ReadAllText(opt.Filename);
    }
    else
    {
        var sb = new StringBuilder();
        string? s;
        while ((s = Console.ReadLine()) != null)
        {
            sb.AppendLine(s);
        }
        src = sb.ToString();
    }

    if (Compiler.TryCompile(opt.Filename != "-" ? opt.Filename : "STDIN", src, out var asm, opt.Trace, opt.Optimize))
    {
        if (opt.Output != "-")
        {
            File.WriteAllText(opt.Output, asm);
        }

        if (opt.Debug || opt.Vt100 || opt.Run)
        {
            var filenameToUse = opt.Output;

            if (opt.Output == "-")
            {
                var tmp = Path.GetTempFileName();
                Console.Error.WriteLine($"writing assembly to temp file {tmp}");
                File.WriteAllText(tmp, asm);
                filenameToUse = tmp;
            }

            if (opt.Debug || opt.Vt100)
            {
                BaseDebugger debugger = opt.Vt100
                   ? new Vt100ConsoleDebugger(filenameToUse, opt.Inputs.ToArray(), [])
                   : new ConsoleDebugger(filenameToUse, opt.Inputs.ToArray(), []);

                debugger.Run(opt.DumpScreen);
            }
            else
            {
                var runner = new Runner(filenameToUse, opt.Inputs.ToArray());
                runner.Run(opt.DumpScreen);
            }
        }
        else
        {
            Console.WriteLine(asm);
        }
    }
    else
    {
        Environment.Exit(-1);
    }
}
