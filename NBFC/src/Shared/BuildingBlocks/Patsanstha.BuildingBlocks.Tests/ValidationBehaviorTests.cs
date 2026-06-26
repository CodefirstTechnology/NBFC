using FluentValidation;
using MediatR;
using Patsanstha.BuildingBlocks.Application.Behaviors;
using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.BuildingBlocks.Tests;

public sealed class ValidationBehaviorTests
{
    private sealed record TestCommand(string Name) : IRequest<Result>;

    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithErrorCode("Name.Required");
        }
    }

    [Fact]
    public async Task Returns_failure_when_validation_fails()
    {
        var behavior = new ValidationBehavior<TestCommand, Result>([new TestCommandValidator()]);
        var nextCalled = false;

        var result = await behavior.Handle(
            new TestCommand(""),
            () =>
            {
                nextCalled = true;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None);

        Assert.False(nextCalled);
        Assert.True(result.IsFailure);
        Assert.Equal("Name.Required", result.Error.Code);
    }

    [Fact]
    public async Task Invokes_next_when_validation_passes()
    {
        var behavior = new ValidationBehavior<TestCommand, Result>([new TestCommandValidator()]);
        var nextCalled = false;

        var result = await behavior.Handle(
            new TestCommand("Valid"),
            () =>
            {
                nextCalled = true;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None);

        Assert.True(nextCalled);
        Assert.True(result.IsSuccess);
    }
}
