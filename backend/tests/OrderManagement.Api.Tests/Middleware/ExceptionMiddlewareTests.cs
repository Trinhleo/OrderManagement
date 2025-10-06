using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Api.Exceptions;
using OrderManagement.Api.Middleware;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OrderManagement.Api.Tests.Middleware;

public class ExceptionMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionMiddleware>> _loggerMock;
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly ExceptionMiddleware _middleware;

    public ExceptionMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _middleware = new ExceptionMiddleware(_nextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _nextMock.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationException_ShouldReturn400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var validationException = new ValidationException("Validation failed");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(validationException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody);

        response.Should().NotBeNull();
        response!.Message.Should().Be("Validation failed");
        response.Details.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_ShouldReturn404()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var notFoundException = new NotFoundException("Resource not found");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(notFoundException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        context.Response.ContentType.Should().Be("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody);

        response.Should().NotBeNull();
        response!.Message.Should().Be("Resource not found");
        response.Details.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_ShouldReturn500()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var genericException = new Exception("Something went wrong");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(genericException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody);

        response.Should().NotBeNull();
        response!.Message.Should().Be("An internal server error occurred");
        response.Details.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationException_ShouldLogError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var validationException = new ValidationException("Validation failed");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(validationException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation error")),
                validationException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_ShouldLogWarning()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var notFoundException = new NotFoundException("Resource not found");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(notFoundException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Not found error")),
                notFoundException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_ShouldLogError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var genericException = new Exception("Something went wrong");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(genericException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unhandled exception")),
                genericException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseAlreadyStarted_ShouldNotModifyResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Simulate response already started
        context.Response.HasStarted.Should().BeFalse(); // Before
        await context.Response.StartAsync();
        context.Response.HasStarted.Should().BeTrue(); // After starting

        var validationException = new ValidationException("Validation failed");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(validationException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        // Should not throw and should not modify the response
        context.Response.StatusCode.Should().Be(200); // Default status code
    }

    private class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}