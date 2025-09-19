namespace Mechanics.Domain.Base;

/// <summary>
///     Classe base para entidades de domínio.
/// </summary>
public abstract class AbstractEntity
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
}
