namespace Mechanics.Domain.Base.Exceptions;

/// <summary>
///     Exceção para entidades não localizadas durante a execução de uma operação.
/// </summary>
public class EntityNotFoundException : BusinessException
{
    private EntityNotFoundException(string entityName, string? key = null)
        : base(FormatMessage(entityName, key))
    {
        EntityName = entityName;
        Key = key;
    }

    public string EntityName { get; }
    public string? Key { get; }

    public static void ThrowIfNull<T>(T? entity, Guid? key) where T : AbstractEntity
    {
        if (entity is not null)
            return;

        throw new EntityNotFoundException(typeof(T).Name, key?.ToString());
    }

    public static void ThrowIfFalse<T>(bool isValid, Guid? key) where T : AbstractEntity
    {
        if (isValid)
            return;

        throw new EntityNotFoundException(typeof(T).Name, key?.ToString());
    }

    private static string FormatMessage(string entityName, string? key)
    {
        return key is null
            ? $"{entityName} not found."
            : $"{entityName} '{key}' not found.";
    }
}
