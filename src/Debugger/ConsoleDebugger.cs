using AnnaSim.Cpu;
using AnnaSim.Exceptions;

namespace AnnaSim.Debugger;

public class ConsoleDebugger
{
    public static void Run(string fname) => Run(fname, [], []);

    public static void Run(string fname, string[] inputs, string[] argv)
    {
        DisplayBanner(fname);

        var cpu = new AnnaMachine(fname, inputs);
        while (true)
        {
            Console.Error.Write("> ");
            var cmd = Console.ReadLine();
            if (cmd == "q")
            {
                break;
            }
            else
            {
                var status = cpu.ExecuteSingleInstruction();

                switch (status)
                {
                    case HaltReason.CyclesExceeded:
                        Console.Error.WriteLine("Allowed cycles exceeded");
                        break;
                    case HaltReason.Halt:
                        Console.Error.WriteLine($"Program halted, PC={cpu.Pc}");
                        break;
                    case HaltReason.Breakpoint:
                        Console.Error.WriteLine($"Hit breakpoint, PC={cpu.Pc}");
                        break;
                }
            }
        }
        Console.Error.WriteLine("Quitting.");
    }

    public static void DisplayBanner(string fname)
    {
        Console.Error.WriteLine("AnnaSim Debugger");
        Console.Error.WriteLine($"  running {fname}");
    }
}