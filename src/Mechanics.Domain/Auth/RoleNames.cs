namespace Mechanics.Domain.Auth;

public static class RoleNames
{
    /// <summary>
    ///     Acesso total ao sistema.
    /// </summary>
    public const string Administrator = "ADMINISTRATOR";

    /// <summary>
    ///     Permite cadastrar clientes e veículos e gerar ordens de serviços.
    /// </summary>
    public const string Attendant = "ATTENDANT";

    /// <summary>
    ///     Permite gerenciar ordens de serviços e realizar manutenções.
    /// </summary>
    public const string Mechanic = "MECHANIC";
}
