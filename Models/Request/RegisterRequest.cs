using System.ComponentModel.DataAnnotations;

namespace comandaAPI.Models.Request;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    public required string UserName { get; set; }
    public string? Email { get; set; } 
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}