using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Patsanstha.BuildingBlocks.Infrastructure.Api;
using Patsanstha.Modules.Accounting.Application.JournalEntries.CreateJournalEntry;
using Patsanstha.Modules.Accounting.Application.JournalEntries.GetJournalEntry;
using Patsanstha.Modules.Accounting.Application.JournalEntries.ListJournalEntries;
using Patsanstha.Modules.Accounting.Application.JournalEntries.PostJournalEntry;
using Patsanstha.Modules.Accounting.Domain.Enums;
using Patsanstha.Modules.Identity.Domain.Authorization;

namespace Patsanstha.Modules.Accounting.Api;

public static class AccountingEndpoints
{
    public static IEndpointRouteBuilder MapAccountingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/accounting").WithTags("Accounting");

        group.MapGet("/", async (
            ISender sender,
            int page = 1,
            int pageSize = 20,
            string? search = null,
            JournalEntryStatus? status = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null) =>
        {
            var result = await sender.Send(new ListJournalEntriesQuery(
                page, pageSize, search, status, fromDate, toDate));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.AccountingRead)
        .WithName("AccountingList");

        group.MapGet("/{journalEntryId:guid}", async (Guid journalEntryId, ISender sender) =>
        {
            var result = await sender.Send(new GetJournalEntryQuery(journalEntryId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.AccountingRead)
        .WithName("AccountingGetById");

        group.MapPost("/", async (CreateJournalEntryRequestBody request, ISender sender) =>
        {
            var command = new CreateJournalEntryCommand(
                request.Description,
                request.EntryDate,
                request.DebitAccountCode,
                request.CreditAccountCode,
                request.Amount,
                request.ReferenceType,
                request.ReferenceId);

            var result = await sender.Send(command);
            return result.IsSuccess
                ? result.ToCreatedResult($"/api/v1/accounting/{result.Value.Id}")
                : result.Error.ToProblemResult();
        })
        .RequireAuthorization(Permissions.AccountingPost)
        .WithName("AccountingCreate");

        group.MapPut("/{journalEntryId:guid}/post", async (Guid journalEntryId, ISender sender) =>
        {
            var result = await sender.Send(new PostJournalEntryCommand(journalEntryId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.AccountingPost)
        .WithName("AccountingPost");

        return app;
    }

    private sealed record CreateJournalEntryRequestBody(
        string Description,
        DateOnly EntryDate,
        string DebitAccountCode,
        string CreditAccountCode,
        decimal Amount,
        string? ReferenceType = null,
        Guid? ReferenceId = null);
}
