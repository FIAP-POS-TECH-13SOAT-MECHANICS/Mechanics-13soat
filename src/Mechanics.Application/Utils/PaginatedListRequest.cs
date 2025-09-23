namespace Mechanics.Application.Utils;

public class PaginatedListRequest
{
    /// <summary>
    ///     Número da página, iniciando em 1.
    /// </summary>
    public required int Page { get; init; } = 1;

    /// <summary>
    ///     Quantidade de itens por página.
    /// </summary>
    public required int ItemsPerPage { get; init; } = 10;
}
