using System.ComponentModel.DataAnnotations;

namespace comandaAPI.Models.Request;

public class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    public required string Username { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}