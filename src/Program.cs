using AnnaSim.Cli;
using AnnaSim.Debugger;
using CommandLine;

var cliParser = new Parser(settings =>
{
    settings.AllowMultiInstance = true;
    settings.HelpWriter = Console.Error;
});

cliParser.ParseArguments<RunCliOptions, DebugCliOptions>(args)
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
    });