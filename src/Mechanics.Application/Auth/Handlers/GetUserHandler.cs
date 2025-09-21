using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Infra.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Auth.Handlers;

public class GetUserHandler(AppDbContext dbContext) : IRequestHandler<GetUserRequest, GetUserResponse?>
{
    public async Task<GetUserResponse?> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user is null)
            return null;

        return new GetUserResponse
        {
            Id = user.Id,
            FullName = user.FullName.ToUpper(),
            UserName = user.UserName.ToLower(),
            Role = new RoleResponse
            {
                Id = user.Role!.Id,
                Name = user.Role.Name,
            },
        };
    }
}
