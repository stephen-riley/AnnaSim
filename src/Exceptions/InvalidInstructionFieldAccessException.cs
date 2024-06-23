using AnnaSim.Instructions;

namespace AnnaSim.Exceptions;

public class InvalidInstructionFieldAccessException : Exception
{
    public InvalidInstructionFieldAccessException(InstructionDefinition idef, string field)
        : base($"Invalid field access {idef.Mnemonic}.{field}" ?? string.Empty) { }

    public InvalidInstructionFieldAccessException(InstructionDefinition idef, string field, string message)
        : base($"Invalid field access {idef.Mnemonic}.{field}: {message}") { }

    public InvalidInstructionFieldAccessException(InstructionDefinition idef, string field, string message, Exception inner)
        : base($"Invalid field access {idef.Mnemonic}.{field}: {message}", inner) { }
}