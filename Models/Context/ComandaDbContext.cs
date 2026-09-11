using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace comandaAPI.Models.Context;

public class ComandaDbContext : IdentityDbContext<IdentityUser>
{
    public ComandaDbContext(DbContextOptions<ComandaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}