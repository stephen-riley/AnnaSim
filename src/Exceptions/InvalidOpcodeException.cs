using AnnaSim.Instructions;

namespace AnnaSim.Exceptions;

public class InvalidOpcodeException : Exception
{
    public InvalidOpcodeException(InstructionDefinition idef)
        : base($"Invalid opcode {idef}" ?? string.Empty) { }

    public InvalidOpcodeException(InstructionDefinition idef, string message)
        : base($"Invalid opcode {idef}: {message}") { }

    public InvalidOpcodeException(InstructionDefinition idef, string message, Exception inner)
        : base($"Invalid opcode {idef}: {message}", inner) { }

    public InvalidOpcodeException(string message)
        : base(message) { }
}