using Mechanics.Application.Utils;

namespace Mechanics.Application.Auth.Responses;

public class GetUsersResponse(IEnumerable<GetUserResponse> items, int totalCount)
    : PaginatedListResponse<GetUserResponse>(items, totalCount);
