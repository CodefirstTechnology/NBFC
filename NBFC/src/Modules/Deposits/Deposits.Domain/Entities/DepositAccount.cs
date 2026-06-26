using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Deposits.Domain.Enums;
using Patsanstha.Modules.Deposits.Domain.Events;

namespace Patsanstha.Modules.Deposits.Domain.Entities;

public sealed class DepositAccount : AggregateRoot
{
    private static readonly Dictionary<DepositAccountStatus, HashSet<DepositAccountStatus>> AllowedTransitions = new()
    {
        [DepositAccountStatus.Active] =
        [
            DepositAccountStatus.Matured,
            DepositAccountStatus.Closed,
            DepositAccountStatus.PrematureClosed,
        ],
        [DepositAccountStatus.Matured] = [DepositAccountStatus.Closed],
        [DepositAccountStatus.Closed] = [],
        [DepositAccountStatus.PrematureClosed] = [],
    };

    public Guid MemberId { get; private set; }

    public string MemberNumber { get; private set; } = string.Empty;

    public string MemberName { get; private set; } = string.Empty;

    public Guid BranchId { get; private set; }

    public string AccountNumber { get; private set; } = string.Empty;

    public DepositProductType ProductType { get; private set; }

    public decimal PrincipalAmount { get; private set; }

    public decimal CurrentBalance { get; private set; }

    public decimal InterestRate { get; private set; }

    public int? TenureMonths { get; private set; }

    public InterestPayoutMode InterestPayoutMode { get; private set; }

    public bool AutoRenewal { get; private set; }

    public DateOnly OpenedOn { get; private set; }

    public DateOnly? MaturityDate { get; private set; }

    public DepositAccountStatus Status { get; private set; }

    private DepositAccount()
    {
    }

    public static DepositAccount Open(
        Guid tenantId,
        Guid memberId,
        string memberNumber,
        string memberName,
        Guid branchId,
        string accountNumber,
        DepositProductType productType,
        decimal principalAmount,
        decimal interestRate,
        int? tenureMonths,
        InterestPayoutMode interestPayoutMode,
        bool autoRenewal,
        DateOnly openedOn)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("TenantId is required.");
        }

        if (memberId == Guid.Empty)
        {
            throw new InvalidOperationException("MemberId is required.");
        }

        if (branchId == Guid.Empty)
        {
            throw new InvalidOperationException("BranchId is required.");
        }

        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new InvalidOperationException("Account number is required.");
        }

        if (principalAmount <= 0)
        {
            throw new InvalidOperationException("Principal amount must be greater than zero.");
        }

        if (interestRate < 0)
        {
            throw new InvalidOperationException("Interest rate cannot be negative.");
        }

        DateOnly? maturityDate = null;

        if (productType is DepositProductType.RecurringDeposit or DepositProductType.FixedDeposit)
        {
            if (!tenureMonths.HasValue || tenureMonths.Value <= 0)
            {
                throw new InvalidOperationException("Tenure months is required for term deposits.");
            }

            maturityDate = openedOn.AddMonths(tenureMonths.Value);
        }
        else if (tenureMonths.HasValue)
        {
            throw new InvalidOperationException("Savings accounts cannot have a fixed tenure.");
        }

        var account = new DepositAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MemberId = memberId,
            MemberNumber = memberNumber.Trim().ToUpperInvariant(),
            MemberName = memberName.Trim(),
            BranchId = branchId,
            AccountNumber = accountNumber.Trim().ToUpperInvariant(),
            ProductType = productType,
            PrincipalAmount = principalAmount,
            CurrentBalance = principalAmount,
            InterestRate = interestRate,
            TenureMonths = tenureMonths,
            InterestPayoutMode = interestPayoutMode,
            AutoRenewal = autoRenewal,
            OpenedOn = openedOn,
            MaturityDate = maturityDate,
            Status = DepositAccountStatus.Active,
        };

        account.RaiseDomainEvent(new DepositAccountOpenedDomainEvent(
            account.Id,
            account.TenantId,
            account.MemberId,
            account.BranchId,
            account.AccountNumber,
            account.ProductType,
            account.PrincipalAmount));

        return account;
    }

    public void ChangeStatus(DepositAccountStatus newStatus)
    {
        if (Status == newStatus)
        {
            return;
        }

        if (!AllowedTransitions[Status].Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition deposit account status from {Status} to {newStatus}.");
        }

        Status = newStatus;
    }

    public void SetAutoRenewal(bool autoRenewal)
    {
        EnsureModifiable();
        AutoRenewal = autoRenewal;
    }

    private void EnsureModifiable()
    {
        if (Status is DepositAccountStatus.Closed or DepositAccountStatus.PrematureClosed)
        {
            throw new InvalidOperationException("Closed deposit accounts cannot be modified.");
        }
    }
}
