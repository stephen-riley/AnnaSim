using AnnaSim.Cpu;

var cpu = new AnnaMachine("../test/files/fibonacci.asm", 5);
var reason = cpu.Execute();
Console.WriteLine(reason);
