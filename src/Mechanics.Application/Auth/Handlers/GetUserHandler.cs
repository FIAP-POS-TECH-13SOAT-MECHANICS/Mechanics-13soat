using AutoMapper;
using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Infra.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Auth.Handlers;

public class GetUserHandler(AppDbContext dbContext, IMapper mapper) : IRequestHandler<GetUserRequest, GetUserResponse?>
{
    public async Task<GetUserResponse?> Handle(GetUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        return user is null ? null : mapper.Map<GetUserResponse>(user);
    }
}
