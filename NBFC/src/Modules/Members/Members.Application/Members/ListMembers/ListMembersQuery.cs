using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Members.Application.Abstractions;
using Patsanstha.Modules.Members.Domain.Enums;

namespace Patsanstha.Modules.Members.Application.Members.ListMembers;

public sealed record ListMembersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    MemberStatus? Status = null,
    Guid? BranchId = null) : IQuery<PagedMembersResponse>;

public sealed class ListMembersQueryValidator : AbstractValidator<ListMembersQuery>
{
    public ListMembersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithErrorCode("Members.Page.Invalid");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("Members.PageSize.Invalid");
    }
}

public sealed class ListMembersQueryHandler(
    IMemberRepository memberRepository,
    IMemberMapper memberMapper) : IQueryHandler<ListMembersQuery, PagedMembersResponse>
{
    public async Task<Result<PagedMembersResponse>> Handle(
        ListMembersQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await memberRepository.ListAsync(
            new ListMembersCriteria(
                request.Page,
                request.PageSize,
                request.Search,
                request.Status,
                request.BranchId),
            cancellationToken);

        var summaries = items.Select(memberMapper.ToSummary).ToList();

        return Result.Success(new PagedMembersResponse(
            summaries,
            request.Page,
            request.PageSize,
            totalCount));
    }
}
