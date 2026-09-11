namespace comandaAPI.Models.DTOs.Response;

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public TokenResponse? Token { get; set; }
}