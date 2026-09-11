using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using comandaAPI.Models.DTOs.Request;
using comandaAPI.Models.DTOs.Response;
using comandaAPI.Models.Identity;
using comandaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using RegisterRequest = comandaAPI.Models.DTOs.Request.RegisterRequest;
using LoginRequest = comandaAPI.Models.DTOs.Request.LoginRequest;

namespace comandaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<ApplicationUser> userManager, ILogger<AuthController> logger, ITokenService tokenService, IConfiguration configuration)
    {
        _userManager = userManager;
        _logger = logger;
        _tokenService = tokenService;
        _configuration = configuration;
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var userExists = await _userManager.FindByNameAsync(request.UserName);
        if (userExists is not null)
        {
            return Unauthorized(new RegisterResponse { Success = false, Message = "User already exists!" });
        }

        ApplicationUser user = new()
        {
            UserName = request.UserName,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogInformation(1, $"Error creating user: {errors}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new RegisterResponse { Success = false, Message = "User creation failed." });
        }
        _logger.LogInformation(1, $"User {user.UserName} created successfully");
        return Ok(new RegisterResponse { Success = true, Message = "User created successfully!" });
    }
    
    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null)
        {
            return Unauthorized(new LoginResponse { Success = false, Message = "Invalid credentials" });
        }
        
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
    
        if (!isPasswordValid)
            return Unauthorized(new LoginResponse { Success = false, Message = "Invalid credentials" });
        
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }
        
        var accessToken = _tokenService.GenerateAccessToken(authClaims, _configuration);
        var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(refreshTokenValidityInDays);
        
        await _userManager.UpdateAsync(user);
        return Ok(new LoginResponse 
        { 
            Success = true,
            Message = "Login successful",
            Token = new TokenResponse 
            { 
                AccessToken = accessTokenString,
                RefreshToken = refreshToken 
            }
        });
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = await _userManager.FindByNameAsync(userName);
    
        if (user == null)
            return BadRequest(new { error = "User not found" });
    
        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Password changed successfully");
    }
    
}