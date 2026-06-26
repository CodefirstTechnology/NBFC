using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Accounting.Domain.Enums;
using Patsanstha.Modules.Accounting.Domain.Events;

namespace Patsanstha.Modules.Accounting.Domain.Entities;

public sealed class JournalEntry : AggregateRoot
{
    public string EntryNumber { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public DateOnly EntryDate { get; private set; }

    public string DebitAccountCode { get; private set; } = string.Empty;

    public string CreditAccountCode { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public JournalEntryStatus Status { get; private set; }

    public string? ReferenceType { get; private set; }

    public Guid? ReferenceId { get; private set; }

    private JournalEntry()
    {
    }

    public static JournalEntry Create(
        Guid tenantId,
        string entryNumber,
        string description,
        DateOnly entryDate,
        string debitAccountCode,
        string creditAccountCode,
        decimal amount,
        string? referenceType = null,
        Guid? referenceId = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("TenantId is required.");
        }

        if (string.IsNullOrWhiteSpace(entryNumber))
        {
            throw new InvalidOperationException("Entry number is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException("Description is required.");
        }

        if (string.IsNullOrWhiteSpace(debitAccountCode))
        {
            throw new InvalidOperationException("Debit account code is required.");
        }

        if (string.IsNullOrWhiteSpace(creditAccountCode))
        {
            throw new InvalidOperationException("Credit account code is required.");
        }

        EnsureAmountPositive(amount);
        EnsureDistinctAccounts(debitAccountCode, creditAccountCode);

        return new JournalEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntryNumber = entryNumber.Trim().ToUpperInvariant(),
            Description = description.Trim(),
            EntryDate = entryDate,
            DebitAccountCode = debitAccountCode.Trim().ToUpperInvariant(),
            CreditAccountCode = creditAccountCode.Trim().ToUpperInvariant(),
            Amount = amount,
            Status = JournalEntryStatus.Draft,
            ReferenceType = referenceType?.Trim(),
            ReferenceId = referenceId,
        };
    }

    public void Post()
    {
        if (Status != JournalEntryStatus.Draft)
        {
            throw new InvalidOperationException("Only draft journal entries can be posted.");
        }

        Status = JournalEntryStatus.Posted;

        RaiseDomainEvent(new JournalEntryPostedDomainEvent(
            Id,
            TenantId,
            EntryNumber,
            DebitAccountCode,
            CreditAccountCode,
            Amount,
            EntryDate,
            ReferenceType,
            ReferenceId));
    }

    public void Reverse()
    {
        if (Status != JournalEntryStatus.Posted)
        {
            throw new InvalidOperationException("Only posted journal entries can be reversed.");
        }

        Status = JournalEntryStatus.Reversed;
    }

    private static void EnsureAmountPositive(decimal amount)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero.");
        }
    }

    private static void EnsureDistinctAccounts(string debitAccountCode, string creditAccountCode)
    {
        if (string.Equals(
                debitAccountCode.Trim(),
                creditAccountCode.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Debit and credit account codes must differ.");
        }
    }
}
