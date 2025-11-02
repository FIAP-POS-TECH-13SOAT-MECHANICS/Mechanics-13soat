using Mechanics.Application.WorkOrders;
using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Application.WorkOrders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
namespace Mechanics.Api.Controllers.WorkOrders;

/// <summary>
///     Controller para gerenciamento de ordens de serviço.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/[controller]")]
public class WorkOrdersController : ControllerBase
{
    private readonly WorkOrderAppService workOrderService;
    private readonly BudgetAppService budgetService;

    public WorkOrdersController(WorkOrderAppService workOrderService, BudgetAppService budgetService)
    {
        this.workOrderService = workOrderService;
        this.budgetService = budgetService;
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
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var id = await workOrderService.Create(request, cancellationToken);
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
        var response = await workOrderService.Get(id, cancellationToken);
        if (response is null) return NotFound();
        return Ok(response);
    }

    /// <summary>
    ///     Solicita aprovação do orçamento para a ordem.
    /// </summary>
    /// <param name="id">Identificador da ordem.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <response code="204">Solicitação realizada.</response>
    /// <response code="400">Requisição inválida.</response>
    [HttpPost("{id:guid}/request-approval")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestApproval(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        await workOrderService.RequestApproval(id, userId.Value, cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Aprova publicamente um budget associado à ordem de serviço.
    ///     Rota pública que o cliente utiliza com seu documento e o código de acesso.
    /// </summary>
    /// <param name="request">Documento e accessKey do cliente.</param>
    /// <param name="cancellationToken">Token para cancelamento.</param>
    [AllowAnonymous]
    [HttpPost("/approve-budget")]
    [Consumes(typeof(ApproveBudgetPublicRequest), "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveBudget([FromBody] ApproveBudgetPublicRequest request, CancellationToken cancellationToken)
    {
        await budgetService.PublicApproveBudget(request.Document, request.AccessKey, cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Altera o status de uma ordem de serviço.
    /// </summary>
    /// <param name="id">Identificador da ordem.</param>
    /// <param name="request">Novo status solicitado</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <response code="204">Status alterado com sucesso.</response>
    /// <response code="400">Requisição inválida.</response>
    [HttpPost("{id:guid}/status")]
    [Consumes(typeof(ChangeStatusRequest), "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        await workOrderService.ChangeStatus(id, request.NewStatus, userId.Value, cancellationToken);
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
    [AllowAnonymous]
    [HttpGet("track")]
    [Produces("application/json", Type = typeof(GetWorkOrderResponse))]
    [ProducesResponseType(typeof(GetWorkOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Track([FromQuery] string document, [FromQuery] string accessKey, CancellationToken cancellationToken)
    {
        var resp = await workOrderService.TrackByDocumentAndAccessKey(document, accessKey, cancellationToken);
        if (resp is null) return NotFound();
        return Ok(resp);
    }

    private Guid? GetCurrentUserId()
    {
        var identifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(identifier, out var userId) ? userId : null;
    }
}
