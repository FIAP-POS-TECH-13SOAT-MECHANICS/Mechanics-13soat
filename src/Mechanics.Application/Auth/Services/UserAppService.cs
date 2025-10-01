using AutoMapper;
using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Utils;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Application.Utils.PagedList;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Auth.Services;

public class UserAppService(AppDbContext dbContext, IMapper mapper) : IAppService
{
    public async Task<CreateItemResponse> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var roleExists = await dbContext.Roles.AnyAsync(r => r.Id == request.RoleId, cancellationToken);
        if (!roleExists)
            throw new KeyNotFoundException("Role not found");

        var entity = mapper.Map<User>(request);

        await dbContext.Users.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateItemResponse { CreatedId = entity.Id };
    }

    public async Task<GetUserResponse?> Get(GetUserRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        return user is null ? null : mapper.Map<GetUserResponse>(user);
    }

    public async Task<GetUsersResponse> GetList(GetUsersRequest request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim().ToUpper();
        var emptyName = string.IsNullOrWhiteSpace(request.Name);

        var query = dbContext.Users
            .Include(user => user.Role)
            .Where(user => emptyName || user.FullName.Contains(normalizedName));

        var (items, count) = await query.GetPaginatedList(request, cancellationToken);
        return new GetUsersResponse(mapper.Map<IEnumerable<GetUserResponse>>(items), count);
    }

    public async Task<UpdateItemResponse?> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null)
            return null;

        user.FullName = request.FullName ?? user.FullName;
        user.UserName = request.UserName ?? user.UserName;
        user.RoleId = request.RoleId ?? user.RoleId;

        await dbContext.SaveChangesAsync(cancellationToken);
        return new UpdateItemResponse();
    }
}
