namespace Patsanstha.Modules.Members.Infrastructure.Options;

public sealed class MemberDocumentStorageOptions
{
    public const string SectionName = "MemberDocuments";

    public string RootPath { get; set; } = "uploads/members";

    public string PublicBasePath { get; set; } = "/api/v1/members/files";
}
