using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Members.Application.Abstractions;

namespace Patsanstha.Modules.Members.Application.Members.SubmitOnboarding;

public sealed record SubmitOnboardingCommand(Guid MemberId) : ICommand<MemberDetailDto>;

public sealed class SubmitOnboardingCommandValidator : AbstractValidator<SubmitOnboardingCommand>
{
    public SubmitOnboardingCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty().WithErrorCode("Members.MemberId.Required");
    }
}

public sealed class SubmitOnboardingCommandHandler(
    IMemberRepository memberRepository,
    IMemberMapper memberMapper) : ICommandHandler<SubmitOnboardingCommand, MemberDetailDto>
{
    public async Task<Result<MemberDetailDto>> Handle(
        SubmitOnboardingCommand request,
        CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdWithDocumentsAsync(request.MemberId, cancellationToken);

        if (member is null)
        {
            return Result.Failure<MemberDetailDto>(
                Error.NotFound("Members.NotFound", "Member not found."));
        }

        try
        {
            member.SubmitOnboarding();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<MemberDetailDto>(
                Error.Validation("Members.Submit.Invalid", ex.Message));
        }

        await memberRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(memberMapper.ToDetail(member));
    }
}
