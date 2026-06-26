using Patsanstha.Modules.Deposits.Domain.Entities;
using Patsanstha.Modules.Deposits.Domain.Enums;
using Patsanstha.Modules.Deposits.Domain.Events;

namespace Patsanstha.Modules.Deposits.Tests;

public sealed class DepositAccountAggregateTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid MemberId = Guid.Parse("00000000-0000-0000-0000-000000000020");
    private static readonly Guid BranchId = Guid.Parse("00000000-0000-0000-0000-000000000010");

    [Fact]
    public void Open_raises_DepositAccountOpened_domain_event()
    {
        var account = CreateFixedDeposit();

        var domainEvent = Assert.Single(account.DomainEvents);
        var opened = Assert.IsType<DepositAccountOpenedDomainEvent>(domainEvent);

        Assert.Equal(account.Id, opened.DepositAccountId);
        Assert.Equal("FD-2026-00001", opened.AccountNumber);
    }

    [Fact]
    public void Open_savings_rejects_tenure()
    {
        Assert.Throws<InvalidOperationException>(() =>
            DepositAccount.Open(
                TenantId,
                MemberId,
                "M202600001",
                "Ramesh Patil",
                BranchId,
                "SB-2026-00001",
                DepositProductType.Savings,
                1000m,
                4.0m,
                12,
                InterestPayoutMode.OnMaturity,
                false,
                DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    [Fact]
    public void Open_term_deposit_requires_tenure()
    {
        Assert.Throws<InvalidOperationException>(() =>
            DepositAccount.Open(
                TenantId,
                MemberId,
                "M202600001",
                "Ramesh Patil",
                BranchId,
                "FD-2026-00001",
                DepositProductType.FixedDeposit,
                50000m,
                7.2m,
                null,
                InterestPayoutMode.OnMaturity,
                false,
                DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    [Fact]
    public void ChangeStatus_allows_valid_transition()
    {
        var account = CreateFixedDeposit();

        account.ChangeStatus(DepositAccountStatus.Matured);

        Assert.Equal(DepositAccountStatus.Matured, account.Status);
    }

    [Fact]
    public void ChangeStatus_rejects_invalid_transition()
    {
        var account = CreateFixedDeposit();
        account.ChangeStatus(DepositAccountStatus.Closed);

        Assert.Throws<InvalidOperationException>(() =>
            account.ChangeStatus(DepositAccountStatus.Active));
    }

    [Fact]
    public void SetAutoRenewal_rejects_when_closed()
    {
        var account = CreateFixedDeposit();
        account.ChangeStatus(DepositAccountStatus.Closed);

        Assert.Throws<InvalidOperationException>(() => account.SetAutoRenewal(true));
    }

    private static DepositAccount CreateFixedDeposit() =>
        DepositAccount.Open(
            TenantId,
            MemberId,
            "M202600001",
            "Ramesh Patil",
            BranchId,
            "FD-2026-00001",
            DepositProductType.FixedDeposit,
            50000m,
            7.2m,
            36,
            InterestPayoutMode.OnMaturity,
            false,
            new DateOnly(2026, 1, 15));
}
