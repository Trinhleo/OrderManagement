using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Api.Controllers;
using OrderManagement.Api.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace OrderManagement.Api.Tests.Controllers;

public class BasicValidationTests
{
    [Fact]
    public void Simple_Test_Should_Pass()
    {
        // Arrange
        var result = 2 + 2;

        // Act & Assert
        result.Should().Be(4);
    }

    [Fact]
    public void AuthController_Should_Create_Successfully()
    {
        // Arrange
        var tokenServiceMock = new Mock<ITokenService>();
        var loggerMock = new Mock<ILogger<AuthController>>();

        // Act
        var controller = new AuthController(tokenServiceMock.Object, loggerMock.Object);

        // Assert
        controller.Should().NotBeNull();
    }

    [Fact]
    public void TokenService_Should_Create_Successfully()
    {
        // Arrange
        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        var jwtSection = new Mock<Microsoft.Extensions.Configuration.IConfigurationSection>();
        jwtSection.Setup(x => x.Value).Returns("your-secret-key-here-must-be-at-least-32-characters-long");
        configMock.Setup(x => x.GetSection("Jwt:SecretKey")).Returns(jwtSection.Object);

        var issuerSection = new Mock<Microsoft.Extensions.Configuration.IConfigurationSection>();
        issuerSection.Setup(x => x.Value).Returns("OrderManagement");
        configMock.Setup(x => x.GetSection("Jwt:Issuer")).Returns(issuerSection.Object);

        var audienceSection = new Mock<Microsoft.Extensions.Configuration.IConfigurationSection>();
        audienceSection.Setup(x => x.Value).Returns("OrderManagement");
        configMock.Setup(x => x.GetSection("Jwt:Audience")).Returns(audienceSection.Object);

        var expirySection = new Mock<Microsoft.Extensions.Configuration.IConfigurationSection>();
        expirySection.Setup(x => x.Value).Returns("60");
        configMock.Setup(x => x.GetSection("Jwt:ExpiryMinutes")).Returns(expirySection.Object);

        // Act
        var tokenService = new TokenService(configMock.Object);

        // Assert
        tokenService.Should().NotBeNull();
    }
}