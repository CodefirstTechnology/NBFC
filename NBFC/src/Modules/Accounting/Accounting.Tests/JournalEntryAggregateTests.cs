using Patsanstha.Modules.Accounting.Domain.Entities;
using Patsanstha.Modules.Accounting.Domain.Enums;
using Patsanstha.Modules.Accounting.Domain.Events;

namespace Patsanstha.Modules.Accounting.Tests;

public sealed class JournalEntryAggregateTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private static JournalEntry CreateDraft() =>
        JournalEntry.Create(
            TenantId,
            "JE-2026-00001",
            "Member deposit receipt",
            new DateOnly(2026, 1, 15),
            "1100",
            "2100",
            5000m);

    [Fact]
    public void Create_starts_in_Draft_status()
    {
        var entry = CreateDraft();

        Assert.Equal(JournalEntryStatus.Draft, entry.Status);
        Assert.Equal("JE-2026-00001", entry.EntryNumber);
        Assert.Equal(5000m, entry.Amount);
        Assert.Empty(entry.DomainEvents);
    }

    [Fact]
    public void Create_rejects_non_positive_amount()
    {
        Assert.Throws<InvalidOperationException>(() =>
            JournalEntry.Create(
                TenantId,
                "JE-2026-00002",
                "Invalid entry",
                new DateOnly(2026, 1, 15),
                "1100",
                "2100",
                0m));
    }

    [Fact]
    public void Create_rejects_same_debit_and_credit_account()
    {
        Assert.Throws<InvalidOperationException>(() =>
            JournalEntry.Create(
                TenantId,
                "JE-2026-00003",
                "Invalid entry",
                new DateOnly(2026, 1, 15),
                "1100",
                "1100",
                1000m));
    }

    [Fact]
    public void Post_transitions_Draft_to_Posted_and_raises_domain_event()
    {
        var entry = CreateDraft();

        entry.Post();

        Assert.Equal(JournalEntryStatus.Posted, entry.Status);

        var domainEvent = Assert.Single(entry.DomainEvents);
        var posted = Assert.IsType<JournalEntryPostedDomainEvent>(domainEvent);

        Assert.Equal(entry.Id, posted.JournalEntryId);
        Assert.Equal("1100", posted.DebitAccountCode);
        Assert.Equal("2100", posted.CreditAccountCode);
        Assert.Equal(5000m, posted.Amount);
    }

    [Fact]
    public void Post_rejects_when_not_Draft()
    {
        var entry = CreateDraft();
        entry.Post();

        Assert.Throws<InvalidOperationException>(() => entry.Post());
    }

    [Fact]
    public void Reverse_transitions_Posted_to_Reversed()
    {
        var entry = CreateDraft();
        entry.Post();

        entry.Reverse();

        Assert.Equal(JournalEntryStatus.Reversed, entry.Status);
    }

    [Fact]
    public void Reverse_rejects_when_not_Posted()
    {
        var entry = CreateDraft();

        Assert.Throws<InvalidOperationException>(() => entry.Reverse());
    }
}
