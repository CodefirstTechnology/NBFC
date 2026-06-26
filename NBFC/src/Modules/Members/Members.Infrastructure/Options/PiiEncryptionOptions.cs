namespace Patsanstha.Modules.Members.Infrastructure.Options;

public sealed class PiiEncryptionOptions
{
    public const string SectionName = "PiiEncryption";

    public string Key { get; set; } = string.Empty;
}
