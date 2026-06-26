using Patsanstha.Modules.Recovery.Application.Abstractions;

namespace Patsanstha.Modules.Recovery.Application.RecoveryCases.GetRecoveryCase;

public sealed record GetRecoveryCaseQuery(Guid RecoveryCaseId) : IQuery<RecoveryCaseDetailDto>;

public sealed class GetRecoveryCaseQueryHandler(
    IRecoveryCaseRepository repository,
    IRecoveryCaseMapper mapper) : IQueryHandler<GetRecoveryCaseQuery, RecoveryCaseDetailDto>
{
    public async Task<Result<RecoveryCaseDetailDto>> Handle(
        GetRecoveryCaseQuery request,
        CancellationToken cancellationToken)
    {
        var recoveryCase = await repository.GetByIdAsync(request.RecoveryCaseId, cancellationToken);

        if (recoveryCase is null)
        {
            return Result.Failure<RecoveryCaseDetailDto>(
                Error.NotFound("Recovery.Case.NotFound", "Recovery case not found."));
        }

        return Result.Success(mapper.ToDetail(recoveryCase));
    }
}
