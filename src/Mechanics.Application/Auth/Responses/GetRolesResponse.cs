using Mechanics.Application.Utils;

namespace Mechanics.Application.Auth.Responses;

public class GetRolesResponse(IEnumerable<RoleResponse> items) : ListResponse<RoleResponse>(items);
