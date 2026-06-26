using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Recovery.Domain.Enums;
using Patsanstha.Modules.Recovery.Domain.Events;

namespace Patsanstha.Modules.Recovery.Domain.Entities;

public sealed class RecoveryCase : AggregateRoot
{
    private static readonly Dictionary<RecoveryCaseStatus, HashSet<RecoveryCaseStatus>> AllowedTransitions = new()
    {
        [RecoveryCaseStatus.Open] =
        [
            RecoveryCaseStatus.InProgress,
            RecoveryCaseStatus.Resolved,
            RecoveryCaseStatus.WrittenOff,
        ],
        [RecoveryCaseStatus.InProgress] =
        [
            RecoveryCaseStatus.Resolved,
            RecoveryCaseStatus.WrittenOff,
        ],
        [RecoveryCaseStatus.Resolved] = [],
        [RecoveryCaseStatus.WrittenOff] = [],
    };

    public Guid LoanApplicationId { get; private set; }

    public string LoanNumber { get; private set; } = string.Empty;

    public Guid MemberId { get; private set; }

    public string MemberNumber { get; private set; } = string.Empty;

    public string MemberName { get; private set; } = string.Empty;

    public Guid BranchId { get; private set; }

    public string CaseNumber { get; private set; } = string.Empty;

    public decimal OutstandingAmount { get; private set; }

    public int DaysPastDue { get; private set; }

    public RecoveryCaseStatus Status { get; private set; }

    public string? Notes { get; private set; }

    public Guid? AssignedToUserId { get; private set; }

    public DateOnly OpenedOn { get; private set; }

    public DateOnly? ResolvedOn { get; private set; }

    private RecoveryCase()
    {
    }

    public static RecoveryCase Open(
        Guid tenantId,
        Guid loanApplicationId,
        string loanNumber,
        Guid memberId,
        string memberNumber,
        string memberName,
        Guid branchId,
        string caseNumber,
        decimal outstandingAmount,
        int daysPastDue,
        string? notes,
        DateOnly openedOn)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("TenantId is required.");
        }

        if (loanApplicationId == Guid.Empty || memberId == Guid.Empty || branchId == Guid.Empty)
        {
            throw new InvalidOperationException("Loan application, member, and branch are required.");
        }

        if (string.IsNullOrWhiteSpace(loanNumber))
        {
            throw new InvalidOperationException("Loan number is required.");
        }

        if (string.IsNullOrWhiteSpace(caseNumber))
        {
            throw new InvalidOperationException("Case number is required.");
        }

        if (outstandingAmount <= 0)
        {
            throw new InvalidOperationException("Outstanding amount must be greater than zero.");
        }

        if (daysPastDue < 0)
        {
            throw new InvalidOperationException("Days past due cannot be negative.");
        }

        var recoveryCase = new RecoveryCase
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LoanApplicationId = loanApplicationId,
            LoanNumber = loanNumber.Trim().ToUpperInvariant(),
            MemberId = memberId,
            MemberNumber = memberNumber.Trim().ToUpperInvariant(),
            MemberName = memberName.Trim(),
            BranchId = branchId,
            CaseNumber = caseNumber.Trim().ToUpperInvariant(),
            OutstandingAmount = outstandingAmount,
            DaysPastDue = daysPastDue,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            Status = RecoveryCaseStatus.Open,
            OpenedOn = openedOn,
        };

        recoveryCase.RaiseDomainEvent(new RecoveryCaseOpenedDomainEvent(
            recoveryCase.Id,
            recoveryCase.TenantId,
            recoveryCase.LoanApplicationId,
            recoveryCase.MemberId,
            recoveryCase.BranchId,
            recoveryCase.CaseNumber,
            recoveryCase.LoanNumber,
            recoveryCase.OutstandingAmount,
            recoveryCase.DaysPastDue));

        return recoveryCase;
    }

    public void UpdateStatus(RecoveryCaseStatus newStatus, DateOnly resolvedOn)
    {
        if (Status == newStatus)
        {
            return;
        }

        if (!AllowedTransitions[Status].Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition recovery case status from {Status} to {newStatus}.");
        }

        Status = newStatus;

        if (newStatus is RecoveryCaseStatus.Resolved or RecoveryCaseStatus.WrittenOff)
        {
            ResolvedOn = resolvedOn;
        }
    }

    public void Assign(Guid assignedToUserId)
    {
        EnsureAssignable();

        if (assignedToUserId == Guid.Empty)
        {
            throw new InvalidOperationException("Assigned user is required.");
        }

        AssignedToUserId = assignedToUserId;
    }

    public void AddNotes(string notes)
    {
        EnsureModifiable();

        if (string.IsNullOrWhiteSpace(notes))
        {
            throw new InvalidOperationException("Notes are required.");
        }

        var trimmed = notes.Trim();
        Notes = string.IsNullOrWhiteSpace(Notes) ? trimmed : $"{Notes}{Environment.NewLine}{trimmed}";
    }

    private void EnsureAssignable()
    {
        if (Status is not (RecoveryCaseStatus.Open or RecoveryCaseStatus.InProgress))
        {
            throw new InvalidOperationException("Only open or in-progress recovery cases can be assigned.");
        }
    }

    private void EnsureModifiable()
    {
        if (Status is RecoveryCaseStatus.Resolved or RecoveryCaseStatus.WrittenOff)
        {
            throw new InvalidOperationException("Closed recovery cases cannot be modified.");
        }
    }
}
