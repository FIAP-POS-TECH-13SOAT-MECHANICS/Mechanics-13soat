using Mechanics.Domain.Customers;

namespace Mechanics.Application.Customers.Requests;

public class CreateCustomerRequest
{
    /// <summary>
    ///     Nome do cliente.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     E-mail de contato do cliente.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    ///     Documento do cliente.
    /// </summary>
    public required PersonalDocumentRequest Document { get; init; }
}

public class PersonalDocumentRequest
{
    /// <summary>
    ///     Tipo do documento (CPF ou CNPJ).
    /// </summary>
    /// <example>cpf</example>
    public required DocumentType? Type { get; init; }

    /// <summary>
    ///     Número do documento.
    /// </summary>
    /// <example>12345678909</example>
    public required string Number { get; init; }
}
