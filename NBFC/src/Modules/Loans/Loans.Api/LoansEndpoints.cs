using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Patsanstha.BuildingBlocks.Infrastructure.Api;
using Patsanstha.Modules.Identity.Domain.Authorization;
using Patsanstha.Modules.Loans.Application.LoanApplications.ApproveLoanApplication;
using Patsanstha.Modules.Loans.Application.LoanApplications.CreateLoanApplication;
using Patsanstha.Modules.Loans.Application.LoanApplications.DisburseLoanApplication;
using Patsanstha.Modules.Loans.Application.LoanApplications.GetLoanApplication;
using Patsanstha.Modules.Loans.Application.LoanApplications.ListLoanApplications;
using Patsanstha.Modules.Loans.Application.LoanApplications.RejectLoanApplication;
using Patsanstha.Modules.Loans.Application.LoanProducts;
using Patsanstha.Modules.Loans.Domain.Enums;
using Patsanstha.BuildingBlocks.Infrastructure.Idempotency;

namespace Patsanstha.Modules.Loans.Api;

public static class LoansEndpoints
{
    public static IEndpointRouteBuilder MapLoansEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/loans").WithTags("Loans");

        group.MapGet("/", async (
            ISender sender,
            int page = 1,
            int pageSize = 20,
            string? search = null,
            LoanProductType? productType = null,
            LoanApplicationStatus? status = null,
            Guid? branchId = null,
            Guid? memberId = null) =>
        {
            var result = await sender.Send(new ListLoanApplicationsQuery(
                page, pageSize, search, productType, status, branchId, memberId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.LoansRead)
        .WithName("LoansList");

        group.MapGet("/products", async (ISender sender) =>
        {
            var result = await sender.Send(new GetLoanProductsQuery());
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.LoansRead)
        .WithName("LoansGetProducts");

        group.MapGet("/{loanApplicationId:guid}", async (Guid loanApplicationId, ISender sender) =>
        {
            var result = await sender.Send(new GetLoanApplicationQuery(loanApplicationId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.LoansRead)
        .WithName("LoansGetById");

        group.MapPost("/", async (CreateLoanApplicationRequestBody request, ISender sender) =>
        {
            var command = new CreateLoanApplicationCommand(
                request.MemberId,
                request.MemberNumber,
                request.MemberName,
                request.BranchId,
                request.ProductType,
                request.RequestedAmount,
                request.TenureMonths,
                request.Purpose);

            var result = await sender.Send(command);
            return result.IsSuccess
                ? result.ToCreatedResult($"/api/v1/loans/{result.Value.Id}")
                : result.Error.ToProblemResult();
        })
        .RequireAuthorization(Permissions.LoansCreate)
        .RequireIdempotency()
        .WithName("LoansCreate");

        group.MapPut("/{loanApplicationId:guid}/approve", async (
            Guid loanApplicationId,
            ApproveLoanApplicationRequestBody request,
            ISender sender) =>
        {
            var result = await sender.Send(new ApproveLoanApplicationCommand(
                loanApplicationId,
                request.ApprovedAmount));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.LoansApprove)
        .WithName("LoansApprove");

        group.MapPut("/{loanApplicationId:guid}/reject", async (
            Guid loanApplicationId,
            RejectLoanApplicationRequestBody request,
            ISender sender) =>
        {
            var result = await sender.Send(new RejectLoanApplicationCommand(
                loanApplicationId,
                request.Reason));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.LoansApprove)
        .WithName("LoansReject");

        group.MapPut("/{loanApplicationId:guid}/disburse", async (
            Guid loanApplicationId,
            ISender sender) =>
        {
            var result = await sender.Send(new DisburseLoanApplicationCommand(loanApplicationId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.LoansDisburse)
        .WithName("LoansDisburse");

        return app;
    }

    private sealed record CreateLoanApplicationRequestBody(
        Guid MemberId,
        string MemberNumber,
        string MemberName,
        Guid BranchId,
        LoanProductType ProductType,
        decimal RequestedAmount,
        int TenureMonths,
        string Purpose);

    private sealed record ApproveLoanApplicationRequestBody(decimal ApprovedAmount);

    private sealed record RejectLoanApplicationRequestBody(string Reason);
}
