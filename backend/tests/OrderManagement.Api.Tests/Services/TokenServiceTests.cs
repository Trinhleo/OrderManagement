using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OrderManagement.Api.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace OrderManagement.Api.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;

    public TokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            ["JwtSettings:SecretKey"] = "MyVeryLongSecretKeyThatIsAtLeast32CharactersLong",
            ["JwtSettings:Issuer"] = "OrderManagement.Api",
            ["JwtSettings:Audience"] = "OrderManagement.Client",
            ["JwtSettings:ExpirationHours"] = "24"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _tokenService = new TokenService(_configuration);
    }

    [Fact]
    public void GenerateJwtToken_WithValidUser_ShouldReturnValidToken()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@example.com";

        // Act
        var token = _tokenService.GenerateJwtToken(userName, email);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        jsonToken.Should().NotBeNull();
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == userName);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        jsonToken.Issuer.Should().Be("OrderManagement.Api");
        jsonToken.Audiences.Should().Contain("OrderManagement.Client");
    }

    [Fact]
    public void GenerateJwtToken_WithEmptyUserName_ShouldThrowArgumentException()
    {
        // Arrange
        var userName = "";
        var email = "test@example.com";

        // Act & Assert
        var action = () => _tokenService.GenerateJwtToken(userName, email);
        action.Should().Throw<ArgumentException>()
            .WithMessage("User name cannot be null or empty*");
    }

    [Fact]
    public void GenerateJwtToken_WithNullUserName_ShouldThrowArgumentException()
    {
        // Arrange
        string userName = null!;
        var email = "test@example.com";

        // Act & Assert
        var action = () => _tokenService.GenerateJwtToken(userName, email);
        action.Should().Throw<ArgumentException>()
            .WithMessage("User name cannot be null or empty*");
    }

    [Fact]
    public void GenerateJwtToken_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var userName = "testuser";
        var email = "";

        // Act & Assert
        var action = () => _tokenService.GenerateJwtToken(userName, email);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be null or empty*");
    }

    [Fact]
    public void GenerateJwtToken_WithNullEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var userName = "testuser";
        string email = null!;

        // Act & Assert
        var action = () => _tokenService.GenerateJwtToken(userName, email);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be null or empty*");
    }

    [Fact]
    public void GenerateJwtToken_ShouldCreateTokenWithCorrectExpiration()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@example.com";

        // Act
        var token = _tokenService.GenerateJwtToken(userName, email);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        var expectedExpiration = DateTime.UtcNow.AddHours(24);
        jsonToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GenerateJwtToken_ShouldCreateUniqueTokensForSameUser()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@example.com";

        // Act
        var token1 = _tokenService.GenerateJwtToken(userName, email);
        var token2 = _tokenService.GenerateJwtToken(userName, email);

        // Assert
        token1.Should().NotBeEquivalentTo(token2);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken1 = tokenHandler.ReadJwtToken(token1);
        var jsonToken2 = tokenHandler.ReadJwtToken(token2);

        var jti1 = jsonToken1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = jsonToken2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        jti1.Should().NotBe(jti2);
    }
}