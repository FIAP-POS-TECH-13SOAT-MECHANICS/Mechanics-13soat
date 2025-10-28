using Mechanics.Application.ServicesCatalog.Requests;
using Mechanics.Application.ServicesCatalog.Responses;
using Mechanics.Application.ServicesCatalog.Services;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Mechanics.Api.Controllers.ServicesCatalog;

/// <summary>
///     Controller para gerenciar os serviços oferecidos pela oficina.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/[controller]")]
[Authorize]
public class ServiceCatalogController(ServiceCatalogAppService service) : ControllerBase
{
    /// <summary>
    ///     Listar os serviços oferecidos pelos filtros informados.
    /// </summary>
    /// <response code="200">Consulta executada.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpGet]
    [Produces("application/json", Type = typeof(GetServiceCatalogResponse))]
    [ProducesResponseType(typeof(GetServiceCatalogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetServiceCatalog([FromQuery] GetServiceCatalogRequest request, CancellationToken cancellationToken = default)
    {
        var response = await service.GetList(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Listar os serviços oferecidos pelo ID.
    /// </summary>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:guid}")]
    [Produces("application/json", Type = typeof(GetServiceCatalogResponse))]
    [ProducesResponseType(typeof(GetServiceCatalogResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServiceCatalog(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await service.Get(id, cancellationToken);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    /// <summary>
    ///     Sugerir os serviços oferecidos com base no tipo de veículo.
    /// </summary>
    /// <param name="vehicleType">Tipo do veículo (ex: SUV, Sedan, Hatch).</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Sugestões geradas com sucesso.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpGet("suggestions")]
    [Produces("application/json", Type = typeof(IEnumerable<GetServicesCatalogResponse>))]
    [ProducesResponseType(typeof(IEnumerable<GetServicesCatalogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSuggestedServices([FromQuery] string vehicleType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vehicleType))
            return BadRequest("O tipo de veículo é obrigatório.");

        var suggestions = await service.GetSuggestions(vehicleType, cancellationToken);
        return Ok(suggestions);
    }

    /// <summary>
    ///     Cadastrar um novo serviço oferecido.
    /// </summary>
    /// <response code="201">Registro cadastrado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes(typeof(CreateServiceCatalogRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(typeof(CreateItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateServiceCatalog([FromBody] CreateServiceCatalogRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await service.Create(request, cancellationToken);
            return CreatedAtAction(nameof(GetServiceCatalog), new { id = response.CreatedId }, response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Atualizar um serviço oferecido existente.
    /// </summary>
    /// <response code="200">Registro atualizado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes(typeof(UpdateServiceCatalogRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(typeof(CreateItemResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateServiceCatalog(Guid id, UpdateServiceCatalogRequest request, CancellationToken cancellationToken = default)
    {
        var response = await service.Update(id, request, cancellationToken);
        return response is null ? NotFound() : NoContent();
    }

    /// <summary>
    ///     Remover um serviço oferecido.
    /// </summary>
    /// <response code="204">Registro deletado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteServiceCatalog(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await service.Delete(id, cancellationToken);

        return response is false ? NotFound() : NoContent();
    }
}
