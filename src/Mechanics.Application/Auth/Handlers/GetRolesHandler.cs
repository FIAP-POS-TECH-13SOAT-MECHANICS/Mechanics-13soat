using AutoMapper;
using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Infra.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Auth.Handlers;

public class GetRolesHandler(AppDbContext dbContext, IMapper mapper) : IRequestHandler<GetRolesRequest, GetRolesResponse>
{
    public async Task<GetRolesResponse> Handle(GetRolesRequest request, CancellationToken cancellationToken = default)
    {
        var roles = await dbContext.Roles
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);

        return new GetRolesResponse(mapper.Map<IEnumerable<RoleResponse>>(roles));
    }
}
