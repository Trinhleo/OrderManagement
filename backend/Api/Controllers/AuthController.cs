using Microsoft.AspNetCore.Mvc;
using OrderManagement.Api.Services;
using System.Security.Claims;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ITokenService tokenService, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for username: {Username}", request.Username);

        // In a real application, validate credentials against a database
        // For demo purposes, we'll use hardcoded credentials
        if (IsValidUser(request.Username, request.Password))
        {
            var roles = GetUserRoles(request.Username);
            var token = _tokenService.GenerateToken(
                GetUserId(request.Username),
                request.Username,
                roles);

            _logger.LogInformation("User {Username} logged in successfully", request.Username);

            return Ok(new LoginResponse
            {
                Token = token,
                Username = request.Username,
                Roles = roles
            });
        }

        _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
        return Unauthorized(new { message = "Invalid credentials" });
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public ActionResult<UserInfo> GetCurrentUser()
    {
        var username = User.Identity?.Name;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Ok(new UserInfo
        {
            Id = userId ?? string.Empty,
            Username = username ?? string.Empty,
            Roles = roles
        });
    }

    /// <summary>
    /// Logout user (client should remove token)
    /// </summary>
    [HttpPost("logout")]
    public ActionResult Logout()
    {
        // In a real application, you might want to blacklist the token
        return Ok(new { message = "Logged out successfully" });
    }

    private bool IsValidUser(string username, string password)
    {
        // Demo credentials - in real app, check against database with hashed passwords
        var validUsers = new Dictionary<string, string>
        {
            { "admin", "admin123" },
            { "user", "user123" },
            { "manager", "manager123" }
        };

        return validUsers.TryGetValue(username.ToLower(), out var validPassword) &&
               validPassword == password;
    }

    private string[] GetUserRoles(string username)
    {
        // Demo role assignment - in real app, get from database
        return username.ToLower() switch
        {
            "admin" => new[] { "Admin", "User" },
            "manager" => new[] { "Manager", "User" },
            _ => new[] { "User" }
        };
    }

    private string GetUserId(string username)
    {
        // Demo user ID - in real app, get from database
        return username.ToLower() switch
        {
            "admin" => "1",
            "manager" => "2",
            "user" => "3",
            _ => Guid.NewGuid().ToString()
        };
    }
}

public record LoginRequest(string Username, string Password);

public record LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
}

public record UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
}