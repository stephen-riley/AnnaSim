using AnnaSim.Debugger;

// Test files:
//   ../test/fixtures/fibonacci.asm
//   ../test/fixtures/mul2_func.asm

// var cpu = new AnnaMachine("../test/fixtures/mul2_func.asm", 6);
// var reason = cpu.Execute(35);

// Console.WriteLine();
// Console.WriteLine(reason);

var debugger = new Vt100ConsoleDebugger("../test/fixtures/print_cstring.asm", [], args);
debugger.Run();

Console.WriteLine("\n\noutput:");
foreach (var w in debugger.Outputs)
{
    Console.WriteLine(w);
}