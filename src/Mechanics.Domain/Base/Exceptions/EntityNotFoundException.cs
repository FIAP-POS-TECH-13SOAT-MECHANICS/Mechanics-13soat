namespace Mechanics.Domain.Base.Exceptions;

/// <summary>
///     Exceção para entidades não localizadas durante a execução de uma operação.
/// </summary>
public class EntityNotFoundException : BusinessException
{
    public EntityNotFoundException(string entityName, string? key = null)
        : base(FormatMessage(entityName, key))
    {
        EntityName = entityName;
        Key = key;
    }

    public string EntityName { get; }
    public string? Key { get; }

    private static string FormatMessage(string entityName, string? key)
    {
        return key is null
            ? $"{entityName} not found."
            : $"{entityName} '{key}' not found.";
    }
}
