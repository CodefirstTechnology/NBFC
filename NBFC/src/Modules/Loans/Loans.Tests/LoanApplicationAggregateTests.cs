using Patsanstha.Modules.Loans.Application.Abstractions;
using Patsanstha.Modules.Loans.Domain.Entities;
using Patsanstha.Modules.Loans.Domain.Enums;
using Patsanstha.Modules.Loans.Domain.Events;

namespace Patsanstha.Modules.Loans.Tests;

public sealed class LoanApplicationAggregateTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid MemberId = Guid.Parse("00000000-0000-0000-0000-000000000020");
    private static readonly Guid BranchId = Guid.Parse("00000000-0000-0000-0000-000000000010");

    private static LoanApplication CreateSubmitted() =>
        LoanApplication.Submit(
            TenantId,
            MemberId,
            "M202600001",
            "Ramesh Patil",
            BranchId,
            "LN-2026-00001",
            LoanProductType.Personal,
            100000m,
            12.0m,
            24,
            "Home renovation",
            new DateOnly(2026, 1, 15));

    [Fact]
    public void Submit_raises_LoanApplicationSubmitted_domain_event()
    {
        var application = CreateSubmitted();

        var domainEvent = Assert.Single(application.DomainEvents);
        var submitted = Assert.IsType<LoanApplicationSubmittedDomainEvent>(domainEvent);

        Assert.Equal(application.Id, submitted.LoanApplicationId);
        Assert.Equal("LN-2026-00001", submitted.LoanNumber);
    }

    [Fact]
    public void Approve_calculates_emi_and_sets_status()
    {
        var application = CreateSubmitted();
        var emi = LoanEmiCalculator.CalculateEmi(100000m, 12.0m, 24);

        application.Approve(100000m, emi, new DateOnly(2026, 1, 20));

        Assert.Equal(LoanApplicationStatus.Approved, application.Status);
        Assert.Equal(emi, application.EmiAmount);
    }

    [Fact]
    public void Disburse_raises_LoanDisbursed_domain_event()
    {
        var application = CreateSubmitted();
        var emi = LoanEmiCalculator.CalculateEmi(100000m, 12.0m, 24);
        application.Approve(100000m, emi, new DateOnly(2026, 1, 20));

        application.Disburse(new DateOnly(2026, 1, 25));

        Assert.Equal(LoanApplicationStatus.Disbursed, application.Status);
        Assert.Equal(100000m, application.OutstandingPrincipal);

        var disbursedEvent = application.DomainEvents.OfType<LoanDisbursedDomainEvent>().Single();
        Assert.Equal(emi, disbursedEvent.EmiAmount);
    }

    [Fact]
    public void Disburse_rejects_when_not_approved()
    {
        var application = CreateSubmitted();

        Assert.Throws<InvalidOperationException>(() =>
            application.Disburse(new DateOnly(2026, 1, 25)));
    }

    [Fact]
    public void Reject_requires_reason()
    {
        var application = CreateSubmitted();

        Assert.Throws<InvalidOperationException>(() =>
            application.Reject("  ", new DateOnly(2026, 1, 20)));
    }
}
