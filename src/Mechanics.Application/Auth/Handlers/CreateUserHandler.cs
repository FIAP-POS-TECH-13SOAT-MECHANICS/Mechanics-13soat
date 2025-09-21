using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Generic;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Auth.Handlers;

public class CreateUserHandler(AppDbContext dbContext) : IRequestHandler<CreateUserRequest, CreateItemResponse>
{
    public async Task<CreateItemResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var roleExists = await dbContext.Roles.AnyAsync(r => r.Id == request.RoleId, cancellationToken);
        if (!roleExists)
            throw new KeyNotFoundException("Role not found");

        var entity = new User
        {
            FullName = request.FullName,
            UserName = request.UserName,
            RoleId = request.RoleId,
        };

        await dbContext.Users.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateItemResponse { CreatedId = entity.Id };
    }
}
