using Mechanics.Application.Auth.Responses;
using MediatR;

namespace Mechanics.Application.Auth.Requests;

public class GetUserRequest : IRequest<GetUserResponse?>
{
    public required Guid Id { get; init; }
}
