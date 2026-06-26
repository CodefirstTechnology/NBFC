using Patsanstha.Modules.Members.Domain.Entities;

namespace Patsanstha.Modules.Members.Application.Abstractions;

public interface IMemberRepository
{
    Task AddAsync(Member member, CancellationToken cancellationToken = default);

    Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByAadhaarHashAsync(string aadhaarHash, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Member> Items, int TotalCount)> ListAsync(
        ListMembersCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IMemberNumberGenerator
{
    Task<string> GenerateNextAsync(CancellationToken cancellationToken = default);
}

public interface IPiiEncryptionService
{
    string Encrypt(string plainText);

    string Decrypt(string cipherText);

    string HashForLookup(string plainText);

    string MaskAadhaar(string plainText);

    string MaskPan(string plainText);
}

public interface IMemberMapper
{
    MemberSummaryDto ToSummary(Member member);

    MemberDetailDto ToDetail(Member member);
}
