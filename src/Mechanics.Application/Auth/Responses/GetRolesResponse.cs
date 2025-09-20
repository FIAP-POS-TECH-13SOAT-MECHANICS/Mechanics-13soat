using Mechanics.Application.Generic;

namespace Mechanics.Application.Auth.Responses;

public class GetRolesResponse(IEnumerable<RoleResponse> items) : ListResponse<RoleResponse>(items);
