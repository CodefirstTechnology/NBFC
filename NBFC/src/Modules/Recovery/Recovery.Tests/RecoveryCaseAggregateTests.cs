using Patsanstha.Modules.Recovery.Domain.Entities;
using Patsanstha.Modules.Recovery.Domain.Enums;
using Patsanstha.Modules.Recovery.Domain.Events;

namespace Patsanstha.Modules.Recovery.Tests;

public sealed class RecoveryCaseAggregateTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid LoanApplicationId = Guid.Parse("00000000-0000-0000-0000-000000000030");
    private static readonly Guid MemberId = Guid.Parse("00000000-0000-0000-0000-000000000020");
    private static readonly Guid BranchId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    private static readonly Guid AssignedUserId = Guid.Parse("00000000-0000-0000-0000-000000000040");

    private static RecoveryCase CreateOpen() =>
        RecoveryCase.Open(
            TenantId,
            LoanApplicationId,
            "LN-2026-00001",
            MemberId,
            "M202600001",
            "Ramesh Patil",
            BranchId,
            "RC-2026-00001",
            50000m,
            45,
            "Initial follow-up required",
            new DateOnly(2026, 1, 15));

    [Fact]
    public void Open_raises_RecoveryCaseOpened_domain_event()
    {
        var recoveryCase = CreateOpen();

        var domainEvent = Assert.Single(recoveryCase.DomainEvents);
        var opened = Assert.IsType<RecoveryCaseOpenedDomainEvent>(domainEvent);

        Assert.Equal(recoveryCase.Id, opened.RecoveryCaseId);
        Assert.Equal("RC-2026-00001", opened.CaseNumber);
        Assert.Equal(50000m, opened.OutstandingAmount);
    }

    [Fact]
    public void UpdateStatus_transitions_from_Open_to_InProgress()
    {
        var recoveryCase = CreateOpen();

        recoveryCase.UpdateStatus(RecoveryCaseStatus.InProgress, new DateOnly(2026, 1, 20));

        Assert.Equal(RecoveryCaseStatus.InProgress, recoveryCase.Status);
        Assert.Null(recoveryCase.ResolvedOn);
    }

    [Fact]
    public void UpdateStatus_sets_ResolvedOn_when_resolved()
    {
        var recoveryCase = CreateOpen();
        var resolvedOn = new DateOnly(2026, 2, 1);

        recoveryCase.UpdateStatus(RecoveryCaseStatus.Resolved, resolvedOn);

        Assert.Equal(RecoveryCaseStatus.Resolved, recoveryCase.Status);
        Assert.Equal(resolvedOn, recoveryCase.ResolvedOn);
    }

    [Fact]
    public void UpdateStatus_rejects_invalid_transition()
    {
        var recoveryCase = CreateOpen();
        recoveryCase.UpdateStatus(RecoveryCaseStatus.Resolved, new DateOnly(2026, 2, 1));

        Assert.Throws<InvalidOperationException>(() =>
            recoveryCase.UpdateStatus(RecoveryCaseStatus.InProgress, new DateOnly(2026, 2, 5)));
    }

    [Fact]
    public void Assign_sets_assigned_user_on_open_case()
    {
        var recoveryCase = CreateOpen();

        recoveryCase.Assign(AssignedUserId);

        Assert.Equal(AssignedUserId, recoveryCase.AssignedToUserId);
    }

    [Fact]
    public void Assign_rejects_when_case_is_resolved()
    {
        var recoveryCase = CreateOpen();
        recoveryCase.UpdateStatus(RecoveryCaseStatus.Resolved, new DateOnly(2026, 2, 1));

        Assert.Throws<InvalidOperationException>(() => recoveryCase.Assign(AssignedUserId));
    }

    [Fact]
    public void AddNotes_appends_to_existing_notes()
    {
        var recoveryCase = CreateOpen();

        recoveryCase.AddNotes("Member promised payment by month end.");

        Assert.Contains("Initial follow-up required", recoveryCase.Notes);
        Assert.Contains("Member promised payment by month end.", recoveryCase.Notes);
    }

    [Fact]
    public void AddNotes_rejects_empty_notes()
    {
        var recoveryCase = CreateOpen();

        Assert.Throws<InvalidOperationException>(() => recoveryCase.AddNotes("  "));
    }

    [Fact]
    public void AddNotes_rejects_when_case_is_written_off()
    {
        var recoveryCase = CreateOpen();
        recoveryCase.UpdateStatus(RecoveryCaseStatus.WrittenOff, new DateOnly(2026, 3, 1));

        Assert.Throws<InvalidOperationException>(() =>
            recoveryCase.AddNotes("Late note attempt"));
    }
}
