using System.Text;
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
    cliParser.ParseArguments<RunCliOptions, DebugCliOptions, CompileOptions>(args)
        .WithParsed<RunCliOptions>(opt =>
        {
            var runner = new Runner(opt.Filename, opt.Inputs.ToArray());
            runner.Run(opt.DumpScreen);
        })
        .WithParsed<DebugCliOptions>(opt =>
        {
            BaseDebugger debugger = opt.Vt100
                ? new Vt100ConsoleDebugger(opt.Filename, opt.Inputs.ToArray(), opt.DebugCommands.ToArray())
                : new ConsoleDebugger(opt.Filename, opt.Inputs.ToArray(), opt.DebugCommands.ToArray());

            debugger.Run(opt.DumpScreen);
        })
        .WithParsed<CompileOptions>(opt =>
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

            if (Compiler.TryCompile(opt.Filename != "-" ? opt.Filename : "STDIN", src, out var asm))
            {
                if (opt.Output != "-")
                {
                    File.WriteAllText(opt.Output, asm);
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
        });
}
catch (Exception e)
{
    Console.Error.WriteLine(e.Message);
}
