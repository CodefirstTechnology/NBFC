using FluentValidation;
using Patsanstha.Modules.Reporting.Application.Abstractions;
using Patsanstha.Modules.Reporting.Application.ReportSnapshots.GenerateReport;
using Patsanstha.Modules.Reporting.Domain.Entities;
using Patsanstha.Modules.Reporting.Domain.Enums;

namespace Patsanstha.Modules.Reporting.Application.ReportSnapshots.GenerateReport;

public sealed record GenerateReportCommand(
    ReportType ReportType,
    string Title,
    string ParametersJson = "{}") : ICommand<ReportSnapshotDetailDto>;

public sealed class GenerateReportCommandValidator : AbstractValidator<GenerateReportCommand>
{
    public GenerateReportCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200).WithErrorCode("Reporting.Title.Required");
        RuleFor(x => x.ParametersJson).NotEmpty().WithErrorCode("Reporting.ParametersJson.Required");
    }
}

public sealed class GenerateReportCommandHandler(
    IReportSnapshotRepository repository,
    IReportSnapshotMapper mapper,
    Patsanstha.BuildingBlocks.Application.Abstractions.Audit.IAuditContextAccessor auditContext)
    : ICommandHandler<GenerateReportCommand, ReportSnapshotDetailDto>
{
    public async Task<Result<ReportSnapshotDetailDto>> Handle(
        GenerateReportCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = auditContext.Current.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<ReportSnapshotDetailDto>(
                Error.Validation("Reporting.Tenant.Required", "Tenant context is required."));
        }

        var resultJson = ReportPlaceholderResults.Build(request.ReportType);

        var snapshot = ReportSnapshot.Generate(
            tenantId,
            request.ReportType,
            request.Title,
            request.ParametersJson,
            resultJson,
            DateTimeOffset.UtcNow,
            auditContext.Current.UserId);

        await repository.AddAsync(snapshot, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDetail(snapshot));
    }
}
