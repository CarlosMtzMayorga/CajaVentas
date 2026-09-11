using CajaVenta.Application.Common;
using FluentAssertions;
using Xunit;

namespace CajaVenta.Domain.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result<string>.Success("test value");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test value");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Act
        var result = Result<string>.Failure("error message");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("error message");
        result.Value.Should().BeNull();
    }

    [Fact]
    public void Success_WithValue_ShouldReturnCorrectValue()
    {
        // Act
        var result = Result<int>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_WithErrorList_ShouldAggregateErrors()
    {
        // Act
        var result = Result<string>.Failure(new List<string> { "error1", "error2" });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("error1; error2");
    }

    [Fact]
    public void Result_NonGeneric_Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Result_NonGeneric_Failure_ShouldCreateFailedResult()
    {
        // Act
        var result = Result.Failure("error message");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("error message");
    }
}