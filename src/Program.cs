using AnnaSim.Cpu;

var cpu = new AnnaMachine("../test/files/fibonacci.asm", 6);
var reason = cpu.Execute(50);
Console.WriteLine(reason);
