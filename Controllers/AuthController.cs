using comandaAPI.Models.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using RegisterRequest = comandaAPI.Models.Request.RegisterRequest;
using LoginRequest = comandaAPI.Models.Request.LoginRequest;

namespace comandaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<IdentityUser> userManager, ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var userExists = await _userManager.FindByNameAsync(request.UserName);
        if (userExists is not null)
        {
            return Unauthorized(new RegisterResponse { Success = false, Message = "User already exists!" });
        }

        IdentityUser user = new()
        {
            UserName = request.UserName,
            Email = request.Email,
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
        
        await _userManager.UpdateAsync(user);
        return Ok(new LoginResponse { Success = true, Message = "User successfully logged in!" });
    }
    
    
}