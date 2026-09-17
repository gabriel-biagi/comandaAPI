using comandaAPI.Models.Context;
using comandaAPI.Models.Identity;
using Microsoft.EntityFrameworkCore;

namespace comandaAPI.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ComandaDbContext _context;

    public RefreshTokenRepository(ComandaDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshTokenEntity?> GetByHashedTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.HashedToken == token && rt.ExpiresAt > DateTime.UtcNow);
    }

    public async Task AddAsync(RefreshTokenEntity entity)
    {
        await _context.RefreshTokens.AddAsync(entity);
    }

    public async Task RemoveAsync(RefreshTokenEntity entity)
    {
        _context.RefreshTokens.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task RemoveByUserIdAsync(string userId)
    {
        var rt = await _context.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.UserId == userId);
        if (rt != null)
        {
            _context.RefreshTokens.Remove(rt);
            await SaveChangesAsync();
        }
    }
}