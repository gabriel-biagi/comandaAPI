using comandaAPI.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace comandaAPI.Models.Context;

public class ComandaDbContext : IdentityDbContext<ApplicationUser>
{
    public ComandaDbContext(DbContextOptions<ComandaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}