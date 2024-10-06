using AnnaSim.AsmParsing;
using AnnaSim.Instructions;

namespace AnnaSim.Debugger;

public class ConsoleDebugger : BaseDebugger
{
    public ConsoleDebugger(CstProgram program, string[] inputs, int screenMap = 0xc000) : this(program, inputs, [], screenMap) { }
    public ConsoleDebugger(CstProgram program, int screenMap = 0xc000) : this(program, [], [], screenMap) { }

    public ConsoleDebugger(CstProgram program, string[] inputs, string[] argv, int screenMap = 0xc000) : base(program, inputs, argv, screenMap)
    {
        Console.CursorVisible = true;
    }

    protected override void Prerun() { }

    protected override void Postrun() { }

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