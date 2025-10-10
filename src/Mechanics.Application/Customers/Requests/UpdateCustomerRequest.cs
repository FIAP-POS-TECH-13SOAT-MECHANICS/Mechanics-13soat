namespace Mechanics.Application.Customers.Requests;

public class UpdateCustomerRequest
{
    /// <summary>
    ///     Nome do cliente. Opcional para atualização.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     E-mail de contato do cliente. Opcional para atualização.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    ///     Documento pessoal do cliente (CPF ou CNPJ). Opcional para atualização.
    /// </summary>
    public PersonalDocumentRequest? Document { get; init; }
}
