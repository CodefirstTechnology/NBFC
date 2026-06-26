using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Members.Application.Abstractions;

namespace Patsanstha.Modules.Members.Application.Members.GetMember;

public sealed record GetMemberQuery(Guid MemberId) : IQuery<MemberDetailDto>;

public sealed class GetMemberQueryValidator : AbstractValidator<GetMemberQuery>
{
    public GetMemberQueryValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty().WithErrorCode("Members.MemberId.Required");
    }
}

public sealed class GetMemberQueryHandler(
    IMemberRepository memberRepository,
    IMemberMapper memberMapper) : IQueryHandler<GetMemberQuery, MemberDetailDto>
{
    public async Task<Result<MemberDetailDto>> Handle(
        GetMemberQuery request,
        CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken);

        if (member is null)
        {
            return Result.Failure<MemberDetailDto>(
                Error.NotFound("Members.NotFound", "Member not found."));
        }

        return Result.Success(memberMapper.ToDetail(member));
    }
}
