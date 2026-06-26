using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Identity.Application.Abstractions;

namespace Patsanstha.Modules.Identity.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string? FullName,
    Guid? BranchId,
    bool? IsActive) : ICommand<UserSummaryDto>;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithErrorCode("Users.UserId.Required");

        When(x => x.FullName is not null, () =>
        {
            RuleFor(x => x.FullName!).NotEmpty().MaximumLength(200).WithErrorCode("Users.FullName.Invalid");
        });
    }
}

public sealed class UpdateUserCommandHandler(IUserAdminService userAdminService)
    : ICommandHandler<UpdateUserCommand, UserSummaryDto>
{
    public Task<Result<UserSummaryDto>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken) =>
        userAdminService.UpdateAsync(
            new UpdateUserRequest(request.UserId, request.FullName, request.BranchId, request.IsActive),
            cancellationToken);
}
