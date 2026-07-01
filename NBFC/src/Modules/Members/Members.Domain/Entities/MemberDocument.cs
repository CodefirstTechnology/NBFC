using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Members.Domain.Enums;

namespace Patsanstha.Modules.Members.Domain.Entities;

public sealed class MemberDocument : AuditableEntity
{
    public Guid MemberId { get; private set; }

    public MemberDocumentType DocumentType { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public string StorageKey { get; private set; } = string.Empty;

    public long FileSizeBytes { get; private set; }

    private MemberDocument()
    {
    }

    public static MemberDocument Create(
        Guid tenantId,
        Guid memberId,
        MemberDocumentType documentType,
        string fileName,
        string contentType,
        string storageKey,
        long fileSizeBytes)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("TenantId is required.");
        }

        if (memberId == Guid.Empty)
        {
            throw new InvalidOperationException("MemberId is required.");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException("File name is required.");
        }

        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new InvalidOperationException("Storage key is required.");
        }

        return new MemberDocument
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MemberId = memberId,
            DocumentType = documentType,
            FileName = fileName.Trim(),
            ContentType = contentType.Trim(),
            StorageKey = storageKey,
            FileSizeBytes = fileSizeBytes,
        };
    }
}
