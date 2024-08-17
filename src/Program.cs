﻿using AnnaSim.Debugger;

// Test files:
//   ../test/fixtures/fibonacci.asm
//   ../test/fixtures/mul2_func.asm

// var cpu = new AnnaMachine("../test/fixtures/mul2_func.asm", 6);
// var reason = cpu.Execute(35);

// Console.WriteLine();
// Console.WriteLine(reason);

var debugger = new Vt100ConsoleDebugger("../test/fixtures/print_cstring_cstr.asm", ["5"], args);
debugger.Run();
