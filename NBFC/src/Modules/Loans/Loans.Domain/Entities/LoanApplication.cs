using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Loans.Domain.Enums;
using Patsanstha.Modules.Loans.Domain.Events;

namespace Patsanstha.Modules.Loans.Domain.Entities;

public sealed class LoanApplication : AggregateRoot
{
    public Guid MemberId { get; private set; }

    public string MemberNumber { get; private set; } = string.Empty;

    public string MemberName { get; private set; } = string.Empty;

    public Guid BranchId { get; private set; }

    public string LoanNumber { get; private set; } = string.Empty;

    public LoanProductType ProductType { get; private set; }

    public decimal RequestedAmount { get; private set; }

    public decimal? ApprovedAmount { get; private set; }

    public decimal InterestRate { get; private set; }

    public int TenureMonths { get; private set; }

    public decimal? EmiAmount { get; private set; }

    public decimal? OutstandingPrincipal { get; private set; }

    public string Purpose { get; private set; } = string.Empty;

    public LoanApplicationStatus Status { get; private set; }

    public string? RejectionReason { get; private set; }

    public DateOnly AppliedOn { get; private set; }

    public DateOnly? ApprovedOn { get; private set; }

    public DateOnly? DisbursedOn { get; private set; }

    private LoanApplication()
    {
    }

    public static LoanApplication Submit(
        Guid tenantId,
        Guid memberId,
        string memberNumber,
        string memberName,
        Guid branchId,
        string loanNumber,
        LoanProductType productType,
        decimal requestedAmount,
        decimal interestRate,
        int tenureMonths,
        string purpose,
        DateOnly appliedOn,
        decimal? estimatedEmiAmount = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("TenantId is required.");
        }

        if (memberId == Guid.Empty || branchId == Guid.Empty)
        {
            throw new InvalidOperationException("Member and branch are required.");
        }

        if (string.IsNullOrWhiteSpace(loanNumber))
        {
            throw new InvalidOperationException("Loan number is required.");
        }

        if (requestedAmount <= 0)
        {
            throw new InvalidOperationException("Requested amount must be greater than zero.");
        }

        if (tenureMonths <= 0)
        {
            throw new InvalidOperationException("Tenure months must be greater than zero.");
        }

        if (interestRate < 0)
        {
            throw new InvalidOperationException("Interest rate cannot be negative.");
        }

        if (estimatedEmiAmount is <= 0)
        {
            throw new InvalidOperationException("Estimated EMI must be greater than zero.");
        }

        var application = new LoanApplication
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MemberId = memberId,
            MemberNumber = memberNumber.Trim().ToUpperInvariant(),
            MemberName = memberName.Trim(),
            BranchId = branchId,
            LoanNumber = loanNumber.Trim().ToUpperInvariant(),
            ProductType = productType,
            RequestedAmount = requestedAmount,
            InterestRate = interestRate,
            TenureMonths = tenureMonths,
            EmiAmount = estimatedEmiAmount,
            Purpose = purpose.Trim(),
            Status = LoanApplicationStatus.Submitted,
            AppliedOn = appliedOn,
        };

        application.RaiseDomainEvent(new LoanApplicationSubmittedDomainEvent(
            application.Id,
            application.TenantId,
            application.MemberId,
            application.BranchId,
            application.LoanNumber,
            application.ProductType,
            application.RequestedAmount));

        return application;
    }

    public void Approve(decimal approvedAmount, decimal emiAmount, DateOnly approvedOn)
    {
        EnsureStatus(LoanApplicationStatus.Submitted, LoanApplicationStatus.UnderReview);

        if (approvedAmount <= 0)
        {
            throw new InvalidOperationException("Approved amount must be greater than zero.");
        }

        if (approvedAmount > RequestedAmount)
        {
            throw new InvalidOperationException("Approved amount cannot exceed requested amount.");
        }

        if (emiAmount <= 0)
        {
            throw new InvalidOperationException("EMI amount must be greater than zero.");
        }

        ApprovedAmount = approvedAmount;
        EmiAmount = emiAmount;
        ApprovedOn = approvedOn;
        RejectionReason = null;
        Status = LoanApplicationStatus.Approved;
    }

    public void Reject(string reason, DateOnly rejectedOn)
    {
        EnsureStatus(LoanApplicationStatus.Submitted, LoanApplicationStatus.UnderReview);

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Rejection reason is required.");
        }

        RejectionReason = reason.Trim();
        ApprovedAmount = null;
        EmiAmount = null;
        ApprovedOn = rejectedOn;
        Status = LoanApplicationStatus.Rejected;
    }

    public void Disburse(DateOnly disbursedOn)
    {
        if (Status != LoanApplicationStatus.Approved)
        {
            throw new InvalidOperationException("Only approved loans can be disbursed.");
        }

        if (!ApprovedAmount.HasValue || !EmiAmount.HasValue)
        {
            throw new InvalidOperationException("Approved amount and EMI must be set before disbursement.");
        }

        OutstandingPrincipal = ApprovedAmount.Value;
        DisbursedOn = disbursedOn;
        Status = LoanApplicationStatus.Disbursed;

        RaiseDomainEvent(new LoanDisbursedDomainEvent(
            Id,
            TenantId,
            MemberId,
            LoanNumber,
            ApprovedAmount.Value,
            EmiAmount.Value));
    }

    public void MarkUnderReview()
    {
        if (Status != LoanApplicationStatus.Submitted)
        {
            throw new InvalidOperationException("Only submitted applications can move to under review.");
        }

        Status = LoanApplicationStatus.UnderReview;
    }

    public void Close()
    {
        if (Status != LoanApplicationStatus.Disbursed)
        {
            throw new InvalidOperationException("Only disbursed loans can be closed.");
        }

        Status = LoanApplicationStatus.Closed;
        OutstandingPrincipal = 0;
    }

    private void EnsureStatus(params LoanApplicationStatus[] allowed)
    {
        if (!allowed.Contains(Status))
        {
            throw new InvalidOperationException(
                $"Operation not allowed when loan application status is {Status}.");
        }
    }
}
