using AnnaSim.Instructions;

namespace AnnaSim.Debugger;

public class ConsoleDebugger : BaseDebugger
{
    public ConsoleDebugger(string fname, string[] inputs, int screenMap = 0xc000) : this(fname, inputs, [], screenMap) { }
    public ConsoleDebugger(string fname, int screenMap = 0xc000) : this(fname, [], [], screenMap) { }

    public ConsoleDebugger(string fname, string[] inputs, string[] argv, int screenMap = 0xc000) : base(fname, inputs, argv, screenMap)
    {
        Console.CursorVisible = true;
    }

    protected override void UpdateScreen(Instruction? instr)
    {
        // maybe show watches here?
        Console.Write(PromptString(instr) + " ");
    }

    protected override void TerminalWrite(string s) => Console.WriteLine(s);

    // protected override string ReadDebuggerCommand() => Console.ReadLine() ?? "";

    protected override string ReadDebuggerCommand()
    {
        string cmd = string.Empty;

        while (true)
        {
            var (left, top) = Console.GetCursorPosition();

            var keyInfo = Console.ReadKey();

            if (keyInfo.Key == ConsoleKey.Backspace && cmd != string.Empty)
            {
                Console.SetCursorPosition(left - 1, top);
                Console.Write(' ');
                Console.SetCursorPosition(left - 1, top);

                cmd = cmd[0..^1];
            }
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                return cmd;
            }
            else
            {
                cmd += keyInfo.KeyChar;
            }
        }
    }
}