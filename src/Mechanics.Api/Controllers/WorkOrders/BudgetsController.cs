using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Domain.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mechanics.Api.Controllers.WorkOrders;

/// <summary>
///     Controller para gerenciamento de ordens de serviço.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/work-orders")]
[Authorize(Policy = PolicyNames.CustomersOnly)]
public class BudgetsController(BudgetAppService budgetService) : ControllerBase
{
    /// <summary>
    ///     Aprova um orçamento associado à ordem de serviço.
    ///     Rota pública que o cliente utiliza com seu documento e o código de acesso.
    /// </summary>
    /// <param name="request">Documento e accessKey do cliente.</param>
    /// <param name="cancellationToken">Token para cancelamento.</param>
    [HttpGet("approve-budget")]
    [Consumes(typeof(BudgetReviewRequest), "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveBudget([FromQuery] BudgetReviewRequest request,
        CancellationToken cancellationToken)
    {
        await budgetService.ApproveBudget(GetCustomerId(), request.AccessKey, request.Description, cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Rejeita um orçamento associado à ordem de serviço.
    ///     Rota pública para o cliente sinalizar que não aceita o orçamento.
    /// </summary>
    [HttpGet("reject-budget")]
    [Consumes(typeof(BudgetReviewRequest), "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectBudget([FromQuery] BudgetReviewRequest request,
        CancellationToken cancellationToken)
    {
        await budgetService.RejectBudget(GetCustomerId(), request.AccessKey, request.Description, cancellationToken);
        return NoContent();
    }

    private Guid GetCustomerId() => Guid.Parse(User.Claims.First(c => c.Type == "customerId").Value);
}
