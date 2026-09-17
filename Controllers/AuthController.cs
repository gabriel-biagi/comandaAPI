using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using comandaAPI.Models.DTOs.Request;
using comandaAPI.Models.DTOs.Response;
using comandaAPI.Models.Identity;
using comandaAPI.Services.Interfaces;
using comandaAPI.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace comandaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly IRefreshTokenRepository _repo;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ITokenService tokenService,
        ILogger<AuthController> logger,
        IRefreshTokenRepository repo)
    {
        _userManager = userManager;
        _configuration = configuration;
        _tokenService = tokenService;
        _logger = logger;
        _repo = repo;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user is null)
                return Unauthorized(new LoginResponse { Success = false, Message = "Invalid credentials" });

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                return Unauthorized(new LoginResponse { Success = false, Message = "Invalid credentials" });

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.UserName, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));

            var accessToken = _tokenService.GenerateAccessToken(authClaims, _configuration);
            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

            Response.Cookies.Append("accessToken", accessTokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });

            var refreshTokenString = _tokenService.GenerateRefreshToken();
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

            var refreshTokenEntity = new RefreshTokenEntity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                HashedToken = refreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenValidityInDays),
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(refreshTokenEntity);
            await _repo.SaveChangesAsync();

            Response.Cookies.Append("refreshToken", refreshTokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(refreshTokenValidityInDays)
            });

            return Ok(new LoginResponse { Success = true, Message = "Login successful" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected login error");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = _userManager.GetUserId(User);
        await _repo.RemoveByUserIdAsync(userId);
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
        return Ok("Logged out successfully");
    }

    [HttpGet("user-logged")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.FindByNameAsync(
            User.FindFirst(ClaimTypes.UserName)?.Value);
        if (user is null)
            return NotFound(new { error = "User not found" });
        return Ok(new { message = $"User is logged in {user.UserName}"});
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            return Unauthorized(new { error = "Refresh token is missing" });

        var refreshTokenEntity = await _repo.GetByHashedTokenAsync(refreshToken);
        if (refreshTokenEntity is null)
            return Unauthorized(new { error = "Invalid or expired refresh token" });

        var user = await _userManager.FindByIdAsync(refreshTokenEntity.UserId);
        if (user is null)
            return Unauthorized(new { error = "User not found" });

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.UserName, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var userRole in userRoles)
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));

        var newAccessToken = _tokenService.GenerateAccessToken(authClaims, _configuration);
        _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);
        var newAccessTokenString = new JwtSecurityTokenHandler().WriteToken(newAccessToken);

        Response.Cookies.Append("accessToken", newAccessTokenString, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(tokenValidityInMinutes)
        });

        await _repo.RemoveAsync(refreshTokenEntity);

        var newRefreshTokenString = _tokenService.GenerateRefreshToken();
        _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

        var newRefreshTokenEntity = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            HashedToken = newRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenValidityInDays),
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(newRefreshTokenEntity);
        await _repo.SaveChangesAsync();

        Response.Cookies.Append("refreshToken", newRefreshTokenString, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(refreshTokenValidityInDays)
        });

        return Ok(new { message = "Access token refreshed successfully" });
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var userExists = await _userManager.FindByNameAsync(request.UserName);
        if (userExists != null)
            return BadRequest(new RegisterResponse { Success = false, Message = "User already exists!" });

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(new RegisterResponse { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) });

        await _userManager.AddToRoleAsync(user, "Employee");

        return Ok(new RegisterResponse { Success = true, Message = "User created successfully" });
    }

    [HttpPost("change-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userName = User.FindFirst(ClaimTypes.UserName)?.Value;
        var user = await _userManager.FindByNameAsync(userName);

        if (user == null)
            return BadRequest(new { error = "User not found" });

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Password changed successfully");
    }
}
