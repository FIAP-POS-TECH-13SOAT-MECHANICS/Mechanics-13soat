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
[Route("work-orders")]
[Authorize(Policy = PolicyNames.CustomersOnly)]
public class BudgetsController(BudgetAppService budgetService) : ControllerBase
{
    /// <summary>
    ///     Aprova um orçamento associado à ordem de serviço.
    /// </summary>
    /// <param name="request">Documento e accessKey do cliente.</param>
    /// <param name="cancellationToken">Token para cancelamento.</param>
    [HttpPost("approve-budget")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveBudget(BudgetReviewRequest request, CancellationToken cancellationToken)
    {
        await budgetService.ApproveBudget(GetCustomerId(), request.AccessKey, request.Description, cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Rejeita um orçamento associado à ordem de serviço.
    /// </summary>
    [HttpPost("reject-budget")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectBudget(BudgetReviewRequest request, CancellationToken cancellationToken)
    {
        await budgetService.RejectBudget(GetCustomerId(), request.AccessKey, request.Description, cancellationToken);
        return NoContent();
    }

    private Guid GetCustomerId() => Guid.Parse(User.Claims.First(c => c.Type == "customerId").Value);
}
