using Mechanics.Application.WorkOrders;
using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Application.WorkOrders.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Mechanics.Api.Controllers.WorkOrders;

/// <summary>
///     Controller para gerenciamento de ordens de serviço.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/[controller]")]
public class WorkOrdersController : ControllerBase
{
    private readonly WorkOrderAppService service;

    public WorkOrdersController(WorkOrderAppService service)
    {
        this.service = service;
    }

    /// <summary>
    ///     Cria uma nova ordem de serviço.
    /// </summary>
    /// <param name="request">Dados da ordem de serviço.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <response code="201">Ordem criada com sucesso.</response>
    /// <response code="400">Requisição inválida.</response>
    [HttpPost]
    [Consumes(typeof(CreateWorkOrderRequest), "application/json")]
    [Produces("application/json", Type = typeof(object))]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request, CancellationToken    cancellationToken)
    {
        var id = await service.Create(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    /// <summary>
    ///     Obtém os detalhes de uma ordem de serviço por id.
    /// </summary>
    /// <param name="id">Identificador da ordem.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:guid}")]
    [Produces("application/json", Type = typeof(GetWorkOrderResponse))]
    [ProducesResponseType(typeof(GetWorkOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var response = await service.Get(id, cancellationToken);
        if (response is null) return NotFound();
        return Ok(response);
    }

    /// <summary>
    ///     Solicita aprovação do orçamento para a ordem.
    /// </summary>
    /// <param name="id">Identificador da ordem.</param>
    /// <param name="performedBy">Id do usuário que está solicitando a aprovação (User.Id).</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <response code="204">Solicitação realizada.</response>
    /// <response code="400">Requisição inválida.</response>
    [HttpPost("{id:guid}/request-approval")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestApproval(Guid id, [FromQuery] Guid performedBy, CancellationToken cancellationToken)
    {
        await service.RequestApproval(id, performedBy, cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Altera o status de uma ordem de serviço.
    /// </summary>
    /// <param name="id">Identificador da ordem.</param>
    /// <param name="request">Novo status e usuário que executou a ação.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <response code="204">Status alterado com sucesso.</response>
    /// <response code="400">Requisição inválida.</response>
    [HttpPost("{id:guid}/status")]
    [Consumes(typeof(ChangeStatusRequest), "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken cancellationToken)
    {
        await service.ChangeStatus(id, request.NewStatus, request.PerformedBy, cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Consulta pública da ordem pelo documento do cliente e chave de acesso.
    /// </summary>
    /// <param name="document">CPF ou CNPJ do cliente (somente dígitos ou formato armazenado).</param>
    /// <param name="accessKey">Código de acesso de 8 dígitos fornecido ao cliente.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <response code="200">Resultado encontrado.</response>
    /// <response code="404">Não encontrado.</response>
    [HttpGet("track")]
    [Produces("application/json", Type = typeof(GetWorkOrderResponse))]
    [ProducesResponseType(typeof(GetWorkOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Track([FromQuery] string document, [FromQuery] string accessKey, CancellationToken cancellationToken)
    {
        var resp = await service.TrackByDocumentAndAccessKey(document, accessKey, cancellationToken);
        if (resp is null) return NotFound();
        return Ok(resp);
    }
}
