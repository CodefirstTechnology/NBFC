using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Collections.Domain.Enums;
using Patsanstha.Modules.Collections.Domain.Events;

namespace Patsanstha.Modules.Collections.Domain.Entities;

public sealed class CollectionReceipt : AggregateRoot
{
    public Guid MemberId { get; private set; }

    public string MemberNumber { get; private set; } = string.Empty;

    public string MemberName { get; private set; } = string.Empty;

    public Guid LoanApplicationId { get; private set; }

    public string LoanNumber { get; private set; } = string.Empty;

    public Guid BranchId { get; private set; }

    public string ReceiptNumber { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public PaymentMode PaymentMode { get; private set; }

    public string? ReferenceNumber { get; private set; }

    public DateOnly CollectedOn { get; private set; }

    public CollectionReceiptStatus Status { get; private set; }

    private CollectionReceipt()
    {
    }

    public static CollectionReceipt Record(
        Guid tenantId,
        Guid memberId,
        string memberNumber,
        string memberName,
        Guid loanApplicationId,
        string loanNumber,
        Guid branchId,
        string receiptNumber,
        decimal amount,
        PaymentMode paymentMode,
        string? referenceNumber,
        DateOnly collectedOn)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("TenantId is required.");
        }

        if (memberId == Guid.Empty || loanApplicationId == Guid.Empty || branchId == Guid.Empty)
        {
            throw new InvalidOperationException("Member, loan application, and branch are required.");
        }

        if (string.IsNullOrWhiteSpace(receiptNumber))
        {
            throw new InvalidOperationException("Receipt number is required.");
        }

        if (string.IsNullOrWhiteSpace(loanNumber))
        {
            throw new InvalidOperationException("Loan number is required.");
        }

        if (amount <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero.");
        }

        var receipt = new CollectionReceipt
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MemberId = memberId,
            MemberNumber = memberNumber.Trim().ToUpperInvariant(),
            MemberName = memberName.Trim(),
            LoanApplicationId = loanApplicationId,
            LoanNumber = loanNumber.Trim().ToUpperInvariant(),
            BranchId = branchId,
            ReceiptNumber = receiptNumber.Trim().ToUpperInvariant(),
            Amount = amount,
            PaymentMode = paymentMode,
            ReferenceNumber = string.IsNullOrWhiteSpace(referenceNumber) ? null : referenceNumber.Trim(),
            CollectedOn = collectedOn,
            Status = CollectionReceiptStatus.Collected,
        };

        receipt.RaiseDomainEvent(new CollectionRecordedDomainEvent(
            receipt.Id,
            receipt.TenantId,
            receipt.MemberId,
            receipt.LoanApplicationId,
            receipt.LoanNumber,
            receipt.ReceiptNumber,
            receipt.Amount,
            receipt.PaymentMode,
            receipt.CollectedOn));

        return receipt;
    }

    public void Reverse()
    {
        if (Status != CollectionReceiptStatus.Collected)
        {
            throw new InvalidOperationException("Only collected receipts can be reversed.");
        }

        Status = CollectionReceiptStatus.Reversed;
    }
}
