using AnnaSim.Instructions;

namespace AnnaSim.Exceptions;

public class InvalidInstructionFieldAccessException : Exception
{
    public InvalidInstructionFieldAccessException(Opcode opcode, string field)
        : base($"Invalid field access {Enum.GetName(opcode)}.{field}" ?? string.Empty) { }

    public InvalidInstructionFieldAccessException(Opcode opcode, string field, string message)
        : base($"Invalid opcode {Enum.GetName(opcode)}.{field}: {message}") { }

    public InvalidInstructionFieldAccessException(Opcode opcode, string field, string message, Exception inner)
        : base($"Invalid opcode {Enum.GetName(opcode)}.{field}: {message}", inner) { }
}