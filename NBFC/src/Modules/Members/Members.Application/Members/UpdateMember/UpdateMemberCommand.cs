using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Members.Application.Abstractions;
using Patsanstha.Modules.Members.Domain.Enums;

namespace Patsanstha.Modules.Members.Application.Members.UpdateMember;

public sealed record UpdateMemberCommand(
    Guid MemberId,
    string FullName,
    string MobileNumber,
    string? Email,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PinCode,
    string? NomineeName,
    string? NomineeRelation,
    MemberStatus? Status) : ICommand<MemberDetailDto>;

public sealed class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty().WithErrorCode("Members.MemberId.Required");
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200).WithErrorCode("Members.FullName.Required");
        RuleFor(x => x.MobileNumber).NotEmpty().Matches(@"^[6-9]\d{9}$").WithErrorCode("Members.Mobile.Invalid");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithErrorCode("Members.Email.Invalid");
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(300).WithErrorCode("Members.Address.Required");
        RuleFor(x => x.City).NotEmpty().MaximumLength(100).WithErrorCode("Members.City.Required");
        RuleFor(x => x.State).NotEmpty().MaximumLength(100).WithErrorCode("Members.State.Required");
        RuleFor(x => x.PinCode).NotEmpty().Matches(@"^\d{6}$").WithErrorCode("Members.PinCode.Invalid");
    }
}

public sealed class UpdateMemberCommandHandler(
    IMemberRepository memberRepository,
    IMemberMapper memberMapper) : ICommandHandler<UpdateMemberCommand, MemberDetailDto>
{
    public async Task<Result<MemberDetailDto>> Handle(
        UpdateMemberCommand request,
        CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken);

        if (member is null)
        {
            return Result.Failure<MemberDetailDto>(
                Error.NotFound("Members.NotFound", "Member not found."));
        }

        try
        {
            member.UpdateProfile(
                request.FullName,
                request.MobileNumber,
                request.Email,
                request.AddressLine1,
                request.AddressLine2,
                request.City,
                request.State,
                request.PinCode,
                request.NomineeName,
                request.NomineeRelation);

            if (request.Status.HasValue)
            {
                member.ChangeStatus(request.Status.Value);
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<MemberDetailDto>(
                Error.Validation("Members.Update.Invalid", ex.Message));
        }

        await memberRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(memberMapper.ToDetail(member));
    }
}
