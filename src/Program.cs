using AnnaSim.Debugger;

// Test files:
//   ../test/fixtures/fibonacci.asm
//   ../test/fixtures/mul2_func.asm

// var cpu = new AnnaMachine("../test/fixtures/mul2_func.asm", 6);
// var reason = cpu.Execute(35);

// Console.WriteLine();
// Console.WriteLine(reason);

var output = ConsoleDebugger.Run("../test/fixtures/multiplication.asm", ["6", "9"], ["d r4", "c", "q"]);

Console.WriteLine("\n\noutput:");
foreach (var w in output)
{
    Console.WriteLine(w);
}