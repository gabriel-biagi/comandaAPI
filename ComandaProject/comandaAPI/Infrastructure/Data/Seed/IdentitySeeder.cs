using comandaAPI.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace comandaAPI.Infrastructure.Data.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        const string adminRole = "Admin";

        string? adminUsername = configuration["Admin:Username"];
        string? adminPassword = configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(adminUsername) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "The 'Admin:Username' and 'Admin:Password' settings are required.");
        }
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole(adminRole));

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    roleResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Unable to create role Admin: {errors}");
            }
        }
        var adminUser = await userManager.FindByNameAsync(adminUsername);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminUsername
            };

            var userResult = await userManager.CreateAsync(
                adminUser,
                adminPassword);

            if (!userResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    userResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Unable to create Admin user: {errors}");
            }
        }
        if (!await userManager.IsInRoleAsync(adminUser, adminRole))
        {
            var roleResult = await userManager.AddToRoleAsync(
                adminUser,
                adminRole);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    roleResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Could not assign to role Admin: {errors}");
            }
        }
    }
}