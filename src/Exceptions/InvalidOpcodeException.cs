using AnnaSim.Cpu.Instructions;

namespace AnnaSim.Exceptions;

public class InvalidOpcodeException : Exception
{
    public InvalidOpcodeException(Opcode opcode)
        : base($"Invalid opcode {Enum.GetName(opcode)}" ?? string.Empty) { }

    public InvalidOpcodeException(Opcode opcode, string message)
        : base($"Invalid opcode {Enum.GetName(opcode)}: {message}") { }

    public InvalidOpcodeException(Opcode opcode, string message, Exception inner)
        : base($"Invalid opcode {Enum.GetName(opcode)}: {message}", inner) { }

    public InvalidOpcodeException(string message)
        : base(message) { }
}