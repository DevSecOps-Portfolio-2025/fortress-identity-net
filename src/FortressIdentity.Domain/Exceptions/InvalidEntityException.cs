namespace FortressIdentity.Domain.Exceptions;

/// <summary>
/// Exception thrown when an entity is in an invalid state.
/// </summary>
public class InvalidEntityException : DomainException
{
    public InvalidEntityException(string entityName, string reason) 
        : base($"Invalid {entityName}: {reason}")
    {
    }
}
