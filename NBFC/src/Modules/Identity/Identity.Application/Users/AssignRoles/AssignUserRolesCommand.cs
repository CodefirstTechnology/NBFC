using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Caching;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Identity.Application.Abstractions;

namespace Patsanstha.Modules.Identity.Application.Users.AssignRoles;

public sealed record AssignUserRolesCommand(
    Guid UserId,
    IReadOnlyList<string> Roles) : ICommand<UserSummaryDto>;

public sealed class AssignUserRolesCommandValidator : AbstractValidator<AssignUserRolesCommand>
{
    public AssignUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithErrorCode("Users.UserId.Required");
        RuleFor(x => x.Roles).NotEmpty().WithErrorCode("Users.Roles.Required");
    }
}

public sealed class AssignUserRolesCommandHandler(
    IUserAdminService userAdminService,
    IReferenceDataCache referenceDataCache)
    : ICommandHandler<AssignUserRolesCommand, UserSummaryDto>
{
    public async Task<Result<UserSummaryDto>> Handle(
        AssignUserRolesCommand request,
        CancellationToken cancellationToken)
    {
        var result = await userAdminService.AssignRolesAsync(
            new AssignUserRolesRequest(request.UserId, request.Roles),
            cancellationToken);

        if (result.IsSuccess)
        {
            referenceDataCache.InvalidateByPrefix(CacheKeys.RolesPrefix);
        }

        return result;
    }
}
