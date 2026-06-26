using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Patsanstha.BuildingBlocks.Infrastructure.Api;
using Patsanstha.Modules.Identity.Domain.Authorization;
using Patsanstha.Modules.Reporting.Application.ReportSnapshots.GenerateReport;
using Patsanstha.Modules.Reporting.Application.ReportSnapshots.GetReportSnapshot;
using Patsanstha.Modules.Reporting.Application.ReportSnapshots.ListReportSnapshots;
using Patsanstha.Modules.Reporting.Domain.Enums;

namespace Patsanstha.Modules.Reporting.Api;

public static class ReportingEndpoints
{
    public static IEndpointRouteBuilder MapReportingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/reports").WithTags("Reporting");

        group.MapGet("/", async (
            ISender sender,
            int page = 1,
            int pageSize = 20,
            ReportType? reportType = null,
            ReportSnapshotStatus? status = null) =>
        {
            var result = await sender.Send(new ListReportSnapshotsQuery(page, pageSize, reportType, status));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.ReportsRead)
        .WithName("ReportsList");

        group.MapGet("/{reportSnapshotId:guid}", async (Guid reportSnapshotId, ISender sender) =>
        {
            var result = await sender.Send(new GetReportSnapshotQuery(reportSnapshotId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.ReportsRead)
        .WithName("ReportsGetById");

        group.MapPost("/generate", async (GenerateReportRequestBody request, ISender sender) =>
        {
            var command = new GenerateReportCommand(
                request.ReportType,
                request.Title,
                request.ParametersJson ?? "{}");

            var result = await sender.Send(command);
            return result.IsSuccess
                ? result.ToCreatedResult($"/api/v1/reports/{result.Value.Id}")
                : result.Error.ToProblemResult();
        })
        .RequireAuthorization(Permissions.ReportsExport)
        .WithName("ReportsGenerate");

        return app;
    }

    private sealed record GenerateReportRequestBody(
        ReportType ReportType,
        string Title,
        string? ParametersJson);
}
