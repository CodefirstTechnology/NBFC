using Patsanstha.Modules.Collections.Domain.Entities;
using Patsanstha.Modules.Collections.Domain.Enums;
using Patsanstha.Modules.Collections.Domain.Events;

namespace Patsanstha.Modules.Collections.Tests;

public sealed class CollectionReceiptAggregateTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid MemberId = Guid.Parse("00000000-0000-0000-0000-000000000020");
    private static readonly Guid BranchId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    private static readonly Guid LoanApplicationId = Guid.Parse("00000000-0000-0000-0000-000000000030");

    private static CollectionReceipt CreateCollected() =>
        CollectionReceipt.Record(
            TenantId,
            MemberId,
            "M202600001",
            "Ramesh Patil",
            LoanApplicationId,
            "LN-2026-00001",
            BranchId,
            "CR-2026-00001",
            5000m,
            PaymentMode.Cash,
            null,
            new DateOnly(2026, 6, 19));

    [Fact]
    public void Record_raises_CollectionRecorded_domain_event()
    {
        var receipt = CreateCollected();

        var domainEvent = Assert.Single(receipt.DomainEvents);
        var recorded = Assert.IsType<CollectionRecordedDomainEvent>(domainEvent);

        Assert.Equal(receipt.Id, recorded.CollectionReceiptId);
        Assert.Equal("CR-2026-00001", recorded.ReceiptNumber);
        Assert.Equal(5000m, recorded.Amount);
    }

    [Fact]
    public void Reverse_changes_status_from_collected_to_reversed()
    {
        var receipt = CreateCollected();

        receipt.Reverse();

        Assert.Equal(CollectionReceiptStatus.Reversed, receipt.Status);
    }

    [Fact]
    public void Reverse_rejects_when_already_reversed()
    {
        var receipt = CreateCollected();
        receipt.Reverse();

        Assert.Throws<InvalidOperationException>(() => receipt.Reverse());
    }

    [Fact]
    public void Record_rejects_non_positive_amount()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CollectionReceipt.Record(
                TenantId,
                MemberId,
                "M202600001",
                "Ramesh Patil",
                LoanApplicationId,
                "LN-2026-00001",
                BranchId,
                "CR-2026-00002",
                0m,
                PaymentMode.Cash,
                null,
                new DateOnly(2026, 6, 19)));
    }
}
