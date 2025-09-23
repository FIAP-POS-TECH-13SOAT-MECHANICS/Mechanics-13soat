using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Utils;
using MediatR;

namespace Mechanics.Application.Auth.Requests;

public class GetUsersRequest : PaginatedListRequest, IRequest<GetUsersResponse>
{
    public string Name { get; init; } = "";
}
