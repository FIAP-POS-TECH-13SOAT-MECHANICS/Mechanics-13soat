using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Infra.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Auth.Handlers;

public class GetRolesHandler(AppDbContext dbContext) : IRequestHandler<GetRolesRequest, GetRolesResponse>
{
    public async Task<GetRolesResponse> Handle(GetRolesRequest request, CancellationToken cancellationToken = default)
    {
        var roles = await dbContext.Roles
            .AsNoTracking()
            .Select(role => new RoleResponse { Id = role.Id, Name = role.Name })
            .ToListAsync(cancellationToken: cancellationToken);

        return new GetRolesResponse(roles);
    }
}
