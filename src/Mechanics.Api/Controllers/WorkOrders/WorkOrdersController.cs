using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Domain.Auth;
using Mechanics.Domain.WorkOrders;
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
[Authorize]
public class WorkOrdersController(WorkOrderAppService workOrderService)
    : ControllerBase
{
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
    public async Task<IActionResult> Create(CreateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var response = await workOrderService.Create(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.CreatedId }, response);
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
    [Authorize(Roles = $"{RoleNames.Mechanic},{RoleNames.Administrator},{RoleNames.Attendant}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestApproval(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await workOrderService.RequestApproval(id, userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Atribui a WorkOrder a um mecânico. (Atendente/Administrator)
    ///     Essa ação grava a atribuição e, se a OS estiver em Received, move automaticamente para UnderDiagnosis.
    /// </summary>
    /// <param name="id">Identificador da ordem.</param>
    /// <param name="request">AssignedToUserId e comentário opcional.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <response code="204">Atribuição realizada.</response>
    /// <response code="400">Requisição inválida.</response>
    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = $"{RoleNames.Attendant},{RoleNames.Administrator}")]
    [Consumes(typeof(AssignWorkOrderRequest), "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await workOrderService.Assign(id, request.AssignedToUserId, userId, request.Description, cancellationToken);
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
    [Authorize]
    [Consumes(typeof(ChangeStatusRequest), "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await workOrderService.ChangeStatus(id, request.NewStatus, userId, request.Description, cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Atualiza produtos, serviços e observações de uma ordem.
    /// </summary>
    /// <param name="id">Identificador da ordem.</param>
    /// <param name="request">Dados de atualização.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <response code="204">Ordem atualizada com sucesso.</response>
    /// <response code="400">Requisição inválida.</response>
    /// <response code="401">Usuário não autenticado.</response>
    [HttpPut("{id:guid}")]
    [Authorize]
    [Consumes(typeof(UpdateWorkOrderRequest), "application/json")]
    [Produces("application/json", Type = typeof(object))]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, UpdateWorkOrderRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await workOrderService.UpdateDetails(id, request, userId, cancellationToken);
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
    public async Task<IActionResult> Track([FromQuery] string document, [FromQuery] string accessKey,
        CancellationToken cancellationToken)
    {
        var response = await workOrderService.TrackByDocumentAndAccessKey(document, accessKey, cancellationToken);
        return response is not null ? Ok(response) : NotFound();
    }

    /// <summary>
    ///     Inicia a execução da OS (Status: InProgress). (Mechanic)
    /// </summary>
    [HttpPost("{id:guid}/start")]
    [Authorize(Roles = $"{RoleNames.Mechanic},{RoleNames.Administrator}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await workOrderService.ChangeStatus(id, WorkOrderStatus.InProgress, userId,
            cancellationToken: cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Marca a OS como concluída (Status: Completed). (Mechanic)
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = $"{RoleNames.Mechanic},{RoleNames.Administrator}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await workOrderService.ChangeStatus(id, WorkOrderStatus.Completed, userId,
            cancellationToken: cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Registra a entrega/retirada do veículo e encerra a OS. (Status: Delivered) (Attendant)
    /// </summary>
    [HttpPost("{id:guid}/deliver")]
    [Authorize(Roles = $"{RoleNames.Attendant},{RoleNames.Administrator}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deliver(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await workOrderService.ChangeStatus(id, WorkOrderStatus.Delivered, userId,
            cancellationToken: cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Lista ordens de serviço com filtros opcionais e paginação.
    /// </summary>
    /// <param name="request">Parâmetros de filtro e paginação.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <response code="200">Consulta executada.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpGet]
    [Produces("application/json", Type = typeof(GetWorkOrdersResponse))]
    [ProducesResponseType(typeof(GetWorkOrdersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWorkOrders([FromQuery] GetWorkOrdersRequest request, CancellationToken cancellationToken)
    {
        var response = await workOrderService.GetList(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Obtém o tempo médio total estimado para execução dos serviços associados à ordem.
    /// </summary>
    /// <param name="id">Identificador da ordem.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:guid}/services/average-time")]
    [Produces("application/json", Type = typeof(GetWorkOrderAverageTimeResponse))]
    [ProducesResponseType(typeof(GetWorkOrderAverageTimeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAverageServiceTime(Guid id, CancellationToken cancellationToken)
    {
        var response = await workOrderService.GetAverageServiceTime(id, cancellationToken);
        return Ok(response);
    }

    private Guid GetCurrentUserId()
    {
        var identifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(identifier, out var userId)
            ? userId
            : throw new InvalidOperationException("User is not authenticated.");
    }
}
