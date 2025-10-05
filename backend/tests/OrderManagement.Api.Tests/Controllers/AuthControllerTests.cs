using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Api.Controllers;
using OrderManagement.Api.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OrderManagement.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_tokenServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = "testuser",
            Password = "password123"
        };

        var expectedToken = "fake-jwt-token";
        _tokenServiceMock
            .Setup(x => x.GenerateJwtToken(loginRequest.UserName, $"{loginRequest.UserName}@example.com"))
            .Returns(expectedToken);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(new { token = expectedToken });
    }

    [Fact]
    public async Task Login_WithEmptyUserName_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = "",
            Password = "password123"
        };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { message = "Username and password are required" });
    }

    [Fact]
    public async Task Login_WithNullUserName_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = null!,
            Password = "password123"
        };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { message = "Username and password are required" });
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = "testuser",
            Password = ""
        };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { message = "Username and password are required" });
    }

    [Fact]
    public async Task Login_WithNullPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = "testuser",
            Password = null!
        };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { message = "Username and password are required" });
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = "invaliduser",
            Password = "wrongpassword"
        };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().BeEquivalentTo(new { message = "Invalid credentials" });
    }

    [Theory]
    [InlineData("admin", "admin123")]
    [InlineData("user", "user123")]
    [InlineData("testuser", "password")]
    public async Task Login_WithValidCredentialCombinations_ShouldReturnOkWithToken(string userName, string password)
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = userName,
            Password = password
        };

        var expectedToken = $"token-for-{userName}";
        _tokenServiceMock
            .Setup(x => x.GenerateJwtToken(userName, $"{userName}@example.com"))
            .Returns(expectedToken);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(new { token = expectedToken });

        _tokenServiceMock.Verify(
            x => x.GenerateJwtToken(userName, $"{userName}@example.com"),
            Times.Once);
    }

    [Fact]
    public async Task Login_WhenTokenServiceThrows_ShouldPropagateException()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = "testuser",
            Password = "password123"
        };

        _tokenServiceMock
            .Setup(x => x.GenerateJwtToken(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("Token service error"));

        // Act & Assert
        var action = async () => await _controller.Login(loginRequest);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Token service error");
    }
}