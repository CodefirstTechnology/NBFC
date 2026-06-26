using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Identity.Application.Abstractions;

namespace Patsanstha.Modules.Identity.Application.Users.GetCurrentUser;

public sealed record GetCurrentUserQuery(Guid UserId) : IQuery<AuthenticatedUserResponse>;

public sealed class GetCurrentUserQueryHandler(IIdentityUserReader userReader)
    : IQueryHandler<GetCurrentUserQuery, AuthenticatedUserResponse>
{
    public Task<Result<AuthenticatedUserResponse>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken) =>
        userReader.GetByIdAsync(request.UserId, cancellationToken);
}
