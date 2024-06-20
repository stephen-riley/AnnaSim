using AnnaSim.Cpu;
using AnnaSim.Cpu.Memory;
using AnnaSim.Exceptions;
using AnnaSim.Extensions;

namespace AnnaSim.Debugger;

public class ConsoleDebugger
{
    public static IEnumerable<Word> Run(string fname) => Run(fname, [], []);

    public static IEnumerable<Word> Run(string fname, string[] inputs, string[] argv)
    {
        DisplayBanner(fname);

        var output = new List<Word>();

        var cpu = new AnnaMachine(fname, inputs)
        {
            OutputCallback = (w) =>
            {
                output.Add(w);
                Console.Error.WriteLine($"out: {w}");
            }
        };

        var status = HaltReason.Running;

        var registersToDisplay = new List<uint>();

        var readFromConsole = argv.Length == 0;
        var cmdQueue = new Queue<string>(argv);


        while (true)
        {
            Console.Error.WriteLine();
            if (registersToDisplay.Count > 0)
            {
                Console.Error.WriteLine(string.Join(" ", registersToDisplay.Select(r => $"r{r}:{cpu.Registers[r]:x4)}")));
            }

            Console.Error.Write($"PC:0x{cpu.Pc:x4} ({cpu.Memory[cpu.Pc].ToInstruction()}) > ");

            string cmd = "";
            if (readFromConsole)
            {
                cmd = Console.ReadLine() ?? "";
            }
            else if (cmdQueue.TryDequeue(out var next))
            {
                cmd = next ?? "";
                Console.Error.WriteLine(cmd);
            }
            else
            {
                return output;
            }

            if (cmd == "q")
            {
                break;
            }
            else if (cmd == "c")
            {
                status = cpu.Execute();
            }
            else if (cmd.StartsWith('r'))
            {
                // TODO: add register aliases to assembler output

                var r = Convert.ToInt32(cmd[1..]);
                if (r < 0 || (r >= cpu.Registers.Length))
                {
                    Console.Error.WriteLine($"* invalid register {cmd} (must be in the range 0..{cpu.Registers.Length - 1})");
                    continue;
                }

                Console.Error.WriteLine($"* {cmd}: {cpu.Registers[Convert.ToUInt16(cmd[1..])]}");
            }
            else if (cmd.StartsWith("m "))
            {
                var addr = Convert.ToUInt16(cmd[2..], 16);
                Console.Error.WriteLine($"* M[{cmd[2..]}]: {cpu.Memory[addr]}");
            }
            else if (cmd == "R")
            {
                cpu.Reset();
                status = HaltReason.Running;
            }
            else if (cmd is "" or "n")
            {
                status = cpu.ExecuteSingleInstruction();
            }
            else if (cmd.StartsWith("d r"))
            {
                var register = Convert.ToUInt16(cmd[3..]);
                registersToDisplay.Add(register);
            }
            else if (cmd == "s")
            {
                Console.Error.WriteLine($"Processor status: {cpu.Status}");
            }
            else
            {
                Console.WriteLine($"invalid command {cmd}");
            }

            switch (status)
            {
                case HaltReason.CyclesExceeded:
                    Console.Error.WriteLine("Allowed cycles exceeded");
                    break;
                case HaltReason.Halt:
                    Console.Error.WriteLine($"Halted, PC: 0x{cpu.Pc:x4}");
                    break;
                case HaltReason.Breakpoint:
                    Console.Error.WriteLine($"Breakpoint, PC: 0x{cpu.Pc:x4}");
                    break;
            }
        }

        Console.Error.WriteLine("Quitting.");
        return output;
    }

    public static void DisplayBanner(string fname)
    {
        Console.Error.WriteLine("AnnaSim Debugger");
        Console.Error.WriteLine($"  running {fname}");
    }
}