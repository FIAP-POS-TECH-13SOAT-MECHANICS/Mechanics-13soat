using AutoMapper;
using Mechanics.Application.Customers.Requests;
using Mechanics.Application.Customers.Responses;
using Mechanics.Application.Utils;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Application.Utils.PagedList;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.Customers;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Customers.Services;

public class CustomerAppService(AppDbContext dbContext, IMapper mapper) : IAppService
{
    public async Task<CreateItemResponse> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Customer>(request);

        if (!entity.IsNormalized())
            entity.Normalize();
        Validator.ValidateAndThrow(entity);

        await dbContext.Customers.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateItemResponse { CreatedId = entity.Id };
    }

    public async Task<GetCustomerResponse?> Get(Guid id, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return customer is null ? null : mapper.Map<GetCustomerResponse>(customer);
    }

    public async Task<GetCustomersResponse> GetList(GetCustomersRequest request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim().ToUpper();
        var emptyName = string.IsNullOrWhiteSpace(request.Name);

        var query = dbContext.Customers
            .Where(c => emptyName || c.Name.Contains(normalizedName));

        var (items, count) = await query.GetPaginatedList(request, cancellationToken);
        return new GetCustomersResponse(mapper.Map<IEnumerable<GetCustomerResponse>>(items), count);
    }

    public async Task<UpdateItemResponse?> Update(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (entity is null)
            return null;

        entity.Name = request.Name ?? entity.Name;
        entity.Email = request.Email ?? entity.Email;

        if (request.Document is not null)
            entity.Document = new PersonalDocument(request.Document.Type!.Value, request.Document.Number);

        if (!entity.IsNormalized())
            entity.Normalize();
        Validator.ValidateAndThrow(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return new UpdateItemResponse();
    }
}
