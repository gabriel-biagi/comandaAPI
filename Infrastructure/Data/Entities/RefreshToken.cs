namespace comandaAPI.Infrastructure.Data.Entities;

public class RefreshTokenEntity
{
    public Guid Id { get; set; }
    public string UserId { get; set; } //FK User
    public ApplicationUser User { get; set; }
    public string HashedToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}