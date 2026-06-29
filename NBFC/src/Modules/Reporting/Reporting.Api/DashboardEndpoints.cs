using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Patsanstha.BuildingBlocks.Infrastructure.Api;
using Patsanstha.Modules.Reporting.Application.Dashboard.GetExecutiveDashboard;

namespace Patsanstha.Modules.Reporting.Api;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/dashboard").WithTags("Dashboard");

        group.MapGet("/executive", async (Guid? branchId, ISender sender) =>
        {
            var result = await sender.Send(new GetExecutiveDashboardQuery(branchId));
            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("DashboardExecutive");

        return app;
    }
}
