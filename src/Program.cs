using AnnaSim.Cli;
using AnnaSim.Debugger;
using AnnaSim.TinyC;
using CommandLine;

var src = """
int fib(int n);

int res = fib(5);
out(res);

int fib(int n) {
	int a = fib(n-1);
	int b = fib(n-2);
    return a + b;
}
""";

if (Compiler.TryCompile(src, out var asm))
{
    Console.WriteLine("success");
}
else
{
    Console.WriteLine("errors");
}
Environment.Exit(0);

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