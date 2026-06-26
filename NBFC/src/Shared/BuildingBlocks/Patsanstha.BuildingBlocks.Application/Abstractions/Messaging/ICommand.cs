namespace Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;

public interface ICommand : MediatR.IRequest<Patsanstha.BuildingBlocks.Domain.Abstractions.Result>;

public interface ICommand<TResponse> : MediatR.IRequest<Patsanstha.BuildingBlocks.Domain.Abstractions.Result<TResponse>>;

public interface IQuery<TResponse> : MediatR.IRequest<Patsanstha.BuildingBlocks.Domain.Abstractions.Result<TResponse>>;

public interface ICommandHandler<in TCommand> : MediatR.IRequestHandler<TCommand, Patsanstha.BuildingBlocks.Domain.Abstractions.Result>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse> : MediatR.IRequestHandler<TCommand, Patsanstha.BuildingBlocks.Domain.Abstractions.Result<TResponse>>
    where TCommand : ICommand<TResponse>;

public interface IQueryHandler<in TQuery, TResponse> : MediatR.IRequestHandler<TQuery, Patsanstha.BuildingBlocks.Domain.Abstractions.Result<TResponse>>
    where TQuery : IQuery<TResponse>;
