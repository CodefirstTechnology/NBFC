using Microsoft.Extensions.Options;
using Patsanstha.Modules.Members.Application.Abstractions;
using Patsanstha.Modules.Members.Infrastructure.Options;

namespace Patsanstha.Modules.Members.Infrastructure.Storage;

public sealed class LocalMemberDocumentStorage(IOptions<MemberDocumentStorageOptions> options) : IMemberDocumentStorage
{
    private readonly MemberDocumentStorageOptions _options = options.Value;

    public async Task<string> SaveAsync(
        Guid memberId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var safeName = Path.GetFileName(fileName);
        var storageKey = $"{memberId:N}/{Guid.NewGuid():N}_{safeName}";
        var fullPath = Path.Combine(_options.RootPath, storageKey.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(fullPath)!;

        Directory.CreateDirectory(directory);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return storageKey;
    }

    public string GetPublicUrl(string storageKey) =>
        $"{_options.PublicBasePath}/{storageKey.Replace('\\', '/')}";

    public string GetPhysicalPath(string storageKey) =>
        Path.Combine(_options.RootPath, storageKey.Replace('/', Path.DirectorySeparatorChar));
}
