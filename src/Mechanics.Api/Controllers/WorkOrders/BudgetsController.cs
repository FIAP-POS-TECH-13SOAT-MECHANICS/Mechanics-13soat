using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Application.WorkOrders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mechanics.Api.Controllers.WorkOrders;

/// <summary>
///     Controller para gerenciamento de ordens de serviço.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/work-orders")]
public class BudgetsController(BudgetAppService budgetService) : ControllerBase
{
    /// <summary>
    ///     Aprova publicamente um budget associado à ordem de serviço.
    ///     Rota pública que o cliente utiliza com seu documento e o código de acesso.
    /// </summary>
    /// <param name="request">Documento e accessKey do cliente.</param>
    /// <param name="cancellationToken">Token para cancelamento.</param>
    [AllowAnonymous]
    [HttpPost("approve-budget")]
    [Consumes(typeof(ApproveBudgetPublicRequest), "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveBudget([FromBody] ApproveBudgetPublicRequest request,
        CancellationToken cancellationToken)
    {
        await budgetService.PublicApproveBudget(request.Document, request.AccessKey, request.Description, cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Rejeita publicamente um budget associado à ordem de serviço.
    ///     Rota pública para o cliente sinalizar que não aceita o orçamento.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("reject-budget")]
    [Consumes(typeof(ApproveBudgetPublicRequest), "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectBudget([FromBody] ApproveBudgetPublicRequest request,
        CancellationToken cancellationToken)
    {
        await budgetService.PublicRejectBudget(request.Document, request.AccessKey, request.Description, cancellationToken);
        return NoContent();
    }
}
