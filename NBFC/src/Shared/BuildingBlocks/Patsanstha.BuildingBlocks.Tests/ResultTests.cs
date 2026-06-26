using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.BuildingBlocks.Tests;

public sealed class ResultTests
{
    [Fact]
    public void Success_result_has_no_error()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Failure_result_carries_error()
    {
        var error = Error.NotFound("Member.NotFound", "Member not found.");
        var result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Typed_success_exposes_value()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Typed_failure_throws_on_value_access()
    {
        var result = Result.Failure<int>(Error.Validation("Invalid", "Invalid value."));

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}
