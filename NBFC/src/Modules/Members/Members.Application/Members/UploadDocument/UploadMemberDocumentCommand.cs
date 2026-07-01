using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Members.Application.Abstractions;
using Patsanstha.Modules.Members.Domain.Enums;

namespace Patsanstha.Modules.Members.Application.Members.UploadDocument;

public sealed record UploadMemberDocumentCommand(
    Guid MemberId,
    MemberDocumentType DocumentType,
    string FileName,
    string ContentType,
    Stream Content,
    long FileSizeBytes) : ICommand<MemberDetailDto>;

public sealed class UploadMemberDocumentCommandValidator : AbstractValidator<UploadMemberDocumentCommand>
{
    private const long MaxPhotoBytes = 2 * 1024 * 1024;
    private const long MaxKycBytes = 5 * 1024 * 1024;

    public UploadMemberDocumentCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty().WithErrorCode("Members.MemberId.Required");
        RuleFor(x => x.FileName).NotEmpty().WithErrorCode("Members.Document.FileName.Required");
        RuleFor(x => x.ContentType).NotEmpty().WithErrorCode("Members.Document.ContentType.Required");
        RuleFor(x => x.FileSizeBytes).GreaterThan(0).WithErrorCode("Members.Document.Empty");
        RuleFor(x => x)
            .Must(x => x.DocumentType != MemberDocumentType.Photo || x.FileSizeBytes <= MaxPhotoBytes)
            .WithErrorCode("Members.Document.Photo.TooLarge");
        RuleFor(x => x)
            .Must(x => x.DocumentType == MemberDocumentType.Photo || x.FileSizeBytes <= MaxKycBytes)
            .WithErrorCode("Members.Document.Kyc.TooLarge");
    }
}

public sealed class UploadMemberDocumentCommandHandler(
    IMemberRepository memberRepository,
    IMemberDocumentStorage documentStorage,
    IMemberMapper memberMapper) : ICommandHandler<UploadMemberDocumentCommand, MemberDetailDto>
{
    public async Task<Result<MemberDetailDto>> Handle(
        UploadMemberDocumentCommand request,
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
            var storageKey = await documentStorage.SaveAsync(
                member.Id,
                request.FileName,
                request.Content,
                cancellationToken);

            member.AddDocument(
                request.DocumentType,
                request.FileName,
                request.ContentType,
                storageKey,
                request.FileSizeBytes);

            if (request.DocumentType == MemberDocumentType.Photo)
            {
                member.SetPhotoStorageKey(storageKey);
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<MemberDetailDto>(
                Error.Validation("Members.Document.Invalid", ex.Message));
        }

        await memberRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(memberMapper.ToDetail(member));
    }
}
