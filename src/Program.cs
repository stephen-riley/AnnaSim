using AnnaSim.Debugger;

// Test files:
//   ../test/files/fibonacci.asm
//   ../test/files/mul2_func.asm

// var cpu = new AnnaMachine("../test/files/mul2_func.asm", 6);
// var reason = cpu.Execute(35);

// Console.WriteLine();
// Console.WriteLine(reason);

ConsoleDebugger.Run("../test/files/fibonacci.asm", ["6"], []);