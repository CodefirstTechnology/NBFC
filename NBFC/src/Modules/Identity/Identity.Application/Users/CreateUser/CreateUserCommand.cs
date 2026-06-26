using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Identity.Application.Abstractions;

namespace Patsanstha.Modules.Identity.Application.Users.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string FullName,
    Guid? BranchId,
    IReadOnlyList<string> Roles) : ICommand<UserSummaryDto>;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithErrorCode("Users.Email.Required");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).WithErrorCode("Users.Password.Required");
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200).WithErrorCode("Users.FullName.Required");
        RuleFor(x => x.Roles).NotEmpty().WithErrorCode("Users.Roles.Required");
    }
}

public sealed class CreateUserCommandHandler(IUserAdminService userAdminService)
    : ICommandHandler<CreateUserCommand, UserSummaryDto>
{
    public Task<Result<UserSummaryDto>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken) =>
        userAdminService.CreateAsync(
            new CreateUserRequest(
                request.Email,
                request.Password,
                request.FullName,
                request.BranchId,
                request.Roles),
            cancellationToken);
}
