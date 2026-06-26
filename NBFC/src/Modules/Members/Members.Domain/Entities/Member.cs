using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Members.Domain.Enums;
using Patsanstha.Modules.Members.Domain.Events;

namespace Patsanstha.Modules.Members.Domain.Entities;

public sealed class Member : AggregateRoot
{
    private static readonly Dictionary<MemberStatus, HashSet<MemberStatus>> AllowedTransitions = new()
    {
        [MemberStatus.Pending] = [MemberStatus.Active, MemberStatus.Inactive],
        [MemberStatus.Active] = [MemberStatus.Inactive, MemberStatus.Suspended, MemberStatus.Closed],
        [MemberStatus.Inactive] = [MemberStatus.Active, MemberStatus.Closed],
        [MemberStatus.Suspended] = [MemberStatus.Active, MemberStatus.Closed],
        [MemberStatus.Closed] = [],
    };

    public Guid BranchId { get; private set; }

    public string MemberNumber { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public DateOnly DateOfBirth { get; private set; }

    public string Gender { get; private set; } = string.Empty;

    public string MobileNumber { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public string AddressLine1 { get; private set; } = string.Empty;

    public string? AddressLine2 { get; private set; }

    public string City { get; private set; } = string.Empty;

    public string State { get; private set; } = string.Empty;

    public string PinCode { get; private set; } = string.Empty;

    public string AadhaarEncrypted { get; private set; } = string.Empty;

    public string PanEncrypted { get; private set; } = string.Empty;

    public string AadhaarHash { get; private set; } = string.Empty;

    public string? NomineeName { get; private set; }

    public string? NomineeRelation { get; private set; }

    public MemberStatus Status { get; private set; }

    public DateOnly JoinedOn { get; private set; }

    private Member()
    {
    }

    public static Member Register(
        Guid tenantId,
        Guid branchId,
        string memberNumber,
        string fullName,
        DateOnly dateOfBirth,
        string gender,
        string mobileNumber,
        string? email,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string pinCode,
        string aadhaarEncrypted,
        string panEncrypted,
        string aadhaarHash,
        string? nomineeName,
        string? nomineeRelation,
        DateOnly joinedOn)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("TenantId is required.");
        }

        if (branchId == Guid.Empty)
        {
            throw new InvalidOperationException("BranchId is required.");
        }

        if (string.IsNullOrWhiteSpace(memberNumber))
        {
            throw new InvalidOperationException("Member number is required.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(aadhaarEncrypted) || string.IsNullOrWhiteSpace(aadhaarHash))
        {
            throw new InvalidOperationException("Aadhaar is required.");
        }

        if (string.IsNullOrWhiteSpace(panEncrypted))
        {
            throw new InvalidOperationException("PAN is required.");
        }

        var member = new Member
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BranchId = branchId,
            MemberNumber = memberNumber.Trim().ToUpperInvariant(),
            FullName = fullName.Trim(),
            DateOfBirth = dateOfBirth,
            Gender = gender.Trim(),
            MobileNumber = mobileNumber.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            AddressLine1 = addressLine1.Trim(),
            AddressLine2 = string.IsNullOrWhiteSpace(addressLine2) ? null : addressLine2.Trim(),
            City = city.Trim(),
            State = state.Trim(),
            PinCode = pinCode.Trim(),
            AadhaarEncrypted = aadhaarEncrypted,
            PanEncrypted = panEncrypted,
            AadhaarHash = aadhaarHash,
            NomineeName = string.IsNullOrWhiteSpace(nomineeName) ? null : nomineeName.Trim(),
            NomineeRelation = string.IsNullOrWhiteSpace(nomineeRelation) ? null : nomineeRelation.Trim(),
            Status = MemberStatus.Active,
            JoinedOn = joinedOn,
        };

        member.RaiseDomainEvent(new MemberRegisteredDomainEvent(
            member.Id,
            member.TenantId,
            member.BranchId,
            member.MemberNumber,
            member.FullName));

        return member;
    }

    public void UpdateProfile(
        string fullName,
        string mobileNumber,
        string? email,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string pinCode,
        string? nomineeName,
        string? nomineeRelation)
    {
        EnsureNotClosed();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        FullName = fullName.Trim();
        MobileNumber = mobileNumber.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        AddressLine1 = addressLine1.Trim();
        AddressLine2 = string.IsNullOrWhiteSpace(addressLine2) ? null : addressLine2.Trim();
        City = city.Trim();
        State = state.Trim();
        PinCode = pinCode.Trim();
        NomineeName = string.IsNullOrWhiteSpace(nomineeName) ? null : nomineeName.Trim();
        NomineeRelation = string.IsNullOrWhiteSpace(nomineeRelation) ? null : nomineeRelation.Trim();
    }

    public void ChangeStatus(MemberStatus newStatus)
    {
        if (Status == newStatus)
        {
            return;
        }

        if (!AllowedTransitions[Status].Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition member status from {Status} to {newStatus}.");
        }

        Status = newStatus;
    }

    private void EnsureNotClosed()
    {
        if (Status == MemberStatus.Closed)
        {
            throw new InvalidOperationException("Closed member accounts cannot be modified.");
        }
    }
}
