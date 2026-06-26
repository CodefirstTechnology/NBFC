using Patsanstha.Modules.Members.Domain.Entities;
using Patsanstha.Modules.Members.Domain.Enums;
using Patsanstha.Modules.Members.Domain.Events;

namespace Patsanstha.Modules.Members.Tests;

public sealed class MemberAggregateTests
{
    private static Member CreateSampleMember() =>
        Member.Register(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000010"),
            "M202600001",
            "Ramesh Patil",
            new DateOnly(1985, 6, 15),
            "Male",
            "9876543210",
            "ramesh@example.com",
            "12 Main Road",
            null,
            "Pune",
            "Maharashtra",
            "411001",
            "encrypted-aadhaar",
            "encrypted-pan",
            "aadhaar-hash",
            "Sunita Patil",
            "Spouse",
            DateOnly.FromDateTime(DateTime.UtcNow));

    [Fact]
    public void Register_raises_MemberRegistered_domain_event()
    {
        var member = CreateSampleMember();

        var domainEvent = Assert.Single(member.DomainEvents);
        var registered = Assert.IsType<MemberRegisteredDomainEvent>(domainEvent);

        Assert.Equal(member.Id, registered.MemberId);
        Assert.Equal("M202600001", registered.MemberNumber);
    }

    [Fact]
    public void ChangeStatus_allows_valid_transition()
    {
        var member = CreateSampleMember();

        member.ChangeStatus(MemberStatus.Suspended);

        Assert.Equal(MemberStatus.Suspended, member.Status);
    }

    [Fact]
    public void ChangeStatus_rejects_invalid_transition()
    {
        var member = CreateSampleMember();
        member.ChangeStatus(MemberStatus.Closed);

        Assert.Throws<InvalidOperationException>(() => member.ChangeStatus(MemberStatus.Active));
    }

    [Fact]
    public void UpdateProfile_rejects_when_member_is_closed()
    {
        var member = CreateSampleMember();
        member.ChangeStatus(MemberStatus.Closed);

        Assert.Throws<InvalidOperationException>(() =>
            member.UpdateProfile(
                "New Name",
                "9876543210",
                null,
                "Address",
                null,
                "Pune",
                "Maharashtra",
                "411001",
                null,
                null));
    }
}

public sealed class PiiEncryptionTests
{
    [Fact]
    public void Encrypt_decrypt_roundtrip_preserves_value()
    {
        var service = new Patsanstha.Modules.Members.Infrastructure.Security.AesPiiEncryptionService(
            Microsoft.Extensions.Options.Options.Create(
                new Patsanstha.Modules.Members.Infrastructure.Options.PiiEncryptionOptions
                {
                    Key = "test-key-for-unit-tests-only",
                }));

        const string plain = "123456789012";

        var encrypted = service.Encrypt(plain);
        var decrypted = service.Decrypt(encrypted);

        Assert.Equal(plain, decrypted);
        Assert.NotEqual(plain, encrypted);
    }

    [Fact]
    public void MaskAadhaar_shows_last_four_digits_only()
    {
        var service = new Patsanstha.Modules.Members.Infrastructure.Security.AesPiiEncryptionService(
            Microsoft.Extensions.Options.Options.Create(
                new Patsanstha.Modules.Members.Infrastructure.Options.PiiEncryptionOptions
                {
                    Key = "test-key-for-unit-tests-only",
                }));

        Assert.Equal("XXXX-XXXX-9012", service.MaskAadhaar("123456789012"));
    }
}
