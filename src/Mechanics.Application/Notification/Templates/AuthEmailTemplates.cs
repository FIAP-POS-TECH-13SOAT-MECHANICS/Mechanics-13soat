using Mechanics.Domain.Auth;
using Mechanics.Infra.Integrations.EmailSender;

namespace Mechanics.Application.Notification.Templates;

/// <summary>
///     Modelos de e-mail para o domínio de autenticação.
/// </summary>
public static class AuthEmailTemplates
{
    public static EmailMessage UserPasswordCreationCode(User user, string passwordCreationCode) => new()
    {
        Recipient = user.Email,
        Subject = "Cadastro de senha - FIAP Mechanics",
        Body = $"""
                <p>Olá, <b>{user.FullName}</b>,</p>
                <p>Utilize o código abaixo para cadastrar sua senha:</p>
                <br />
                <code>{passwordCreationCode}</code>
                """,
    };

    public static EmailMessage UserPasswordChanged(User user) => new()
    {
        Recipient = user.Email,
        Subject = "Senha alterada - FIAP Mechanics",
        Body = $"""
                <p>Olá, <b>{user.FullName}</b>,</p>
                <p>Sua senha foi alterada hoje, {DateTime.Now:dd/MM}, às {DateTime.Now:HH:mm}.</p>
                <p>Por segurança, você foi desconectado de todos os seus dispositivos.</p>
                """,
    };
}
