using FluentValidation;
using MediatR;
using Xunit;

namespace SofiaTransport.Application.Tests.Common.Behaviors;

public class ValidationBehaviorTests
{
    public record TestRequest(string Name, int Age) : IRequest<TestResponse>;
    public record TestResponse(string Value);

    /// <summary>Validator that always passes.</summary>
    private class PassValidator : AbstractValidator<TestRequest>
    {
        public PassValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    /// <summary>Validator that always fails.</summary>
    private class FailValidator : AbstractValidator<TestRequest>
    {
        public FailValidator()
        {
            RuleFor(x => x.Age).GreaterThan(100).WithMessage("Age must be over 100.");
        }
    }

    [Fact]
    public async Task Handle_NoValidatorsRegistered_CallsNextAndReturnsResponse()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new SofiaTransport.Application.Common.Behaviors.ValidationBehavior<TestRequest, TestResponse>(validators);

        var expectedResponse = new TestResponse("ok");
        var request = new TestRequest("John", 30);

        var next = new RequestHandlerDelegate<TestResponse>(_ => Task.FromResult(expectedResponse));

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public async Task Handle_ValidRequest_AllValidatorsPass_CallsNext()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new PassValidator() };
        var behavior = new SofiaTransport.Application.Common.Behaviors.ValidationBehavior<TestRequest, TestResponse>(validators);

        var expectedResponse = new TestResponse("ok");
        var request = new TestRequest("John", 30);

        var next = new RequestHandlerDelegate<TestResponse>(_ => Task.FromResult(expectedResponse));

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public async Task Handle_InvalidRequest_ValidatorsReturnFailures_ThrowsValidationException()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new FailValidator() };
        var behavior = new SofiaTransport.Application.Common.Behaviors.ValidationBehavior<TestRequest, TestResponse>(validators);

        var request = new TestRequest("John", 50);
        bool nextCalled = false;
        var next = new RequestHandlerDelegate<TestResponse>(_ =>
        {
            nextCalled = true;
            return Task.FromResult(new TestResponse("should-not-reach"));
        });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(request, next, CancellationToken.None));

        Assert.Contains("Age", ex.Errors.Select(e => e.PropertyName));
        Assert.False(nextCalled);
    }
}
