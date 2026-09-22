using comandaAPI.Infrastructure.Data.Entities;

namespace comandaAPI.Infrastructure.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshTokenEntity?> GetByHashedTokenAsync(string hashedToken);
    Task AddAsync(RefreshTokenEntity entity);
    Task RemoveAsync(RefreshTokenEntity entity);
    Task SaveChangesAsync();
    Task RemoveByUserIdAsync(string userId);
}
