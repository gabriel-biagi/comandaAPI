using System.ComponentModel.DataAnnotations;

namespace comandaAPI.Models.DTOs.Request;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3)]
    public required string UserName { get; set; }
    public string? Email { get; set; } 
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8)]
    public required string Password { get; set; }
}