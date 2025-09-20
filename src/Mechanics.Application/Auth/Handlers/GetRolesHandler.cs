using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Infra.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Auth.Handlers;

public class GetRolesHandler(AppDbContext dbContext) : IRequestHandler<GetRolesRequest, GetRolesResponse>
{
    public async Task<GetRolesResponse> Handle(GetRolesRequest request, CancellationToken cancellationToken)
    {
        var roles = await dbContext.Roles.ToListAsync(cancellationToken: cancellationToken);
        var roleResponses = roles.Select(role => new RoleResponse { Id = role.Id, Name = role.Name });

        return new GetRolesResponse(roleResponses);
    }
}
