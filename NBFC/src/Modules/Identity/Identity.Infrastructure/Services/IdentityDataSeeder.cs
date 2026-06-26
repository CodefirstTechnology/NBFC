using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Patsanstha.Modules.Identity.Domain.Authorization;
using Patsanstha.Modules.Identity.Infrastructure.Identity;
using Patsanstha.Modules.Identity.Infrastructure.Options;
using Patsanstha.Modules.Identity.Infrastructure.Persistence;

namespace Patsanstha.Modules.Identity.Infrastructure.Services;

public sealed class IdentityDataSeeder(
    IdentityDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<IdentitySeedOptions> seedOptions,
    ILogger<IdentityDataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var pending = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pending.Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else if (!(await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken)).Any())
        {
            // Dev bootstrap before the first EF migration is generated.
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        foreach (var roleName in SystemRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    Description = $"{roleName} system role",
                });
            }
        }

        foreach (var (roleName, permissions) in SystemRoles.DefaultRolePermissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }

            var existingClaims = await roleManager.GetClaimsAsync(role);

            foreach (var permission in permissions)
            {
                if (existingClaims.Any(c => c.Type == Permissions.ClaimType && c.Value == permission))
                {
                    continue;
                }

                await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(Permissions.ClaimType, permission));
            }
        }

        var options = seedOptions.Value;
        var admin = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == options.AdminEmail, cancellationToken);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = options.AdminEmail,
                UserName = options.AdminEmail,
                NormalizedEmail = options.AdminEmail.ToUpperInvariant(),
                NormalizedUserName = options.AdminEmail.ToUpperInvariant(),
                FullName = options.AdminFullName,
                TenantId = options.DefaultTenantId,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            var createResult = await userManager.CreateAsync(admin, options.AdminPassword);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to seed admin user: {Errors}", errors);
                return;
            }

            await userManager.AddToRoleAsync(admin, SystemRoles.SystemAdmin);
            logger.LogInformation("Seeded admin user {Email}", options.AdminEmail);
            return;
        }

        var changed = false;

        if (admin.IsDeleted)
        {
            admin.IsDeleted = false;
            changed = true;
        }

        if (!admin.IsActive)
        {
            admin.IsActive = true;
            changed = true;
        }

        if (!admin.EmailConfirmed)
        {
            admin.EmailConfirmed = true;
            changed = true;
        }

        if (changed)
        {
            await userManager.UpdateAsync(admin);
        }

        await userManager.SetLockoutEndDateAsync(admin, null);
        await userManager.ResetAccessFailedCountAsync(admin);

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
        var resetResult = await userManager.ResetPasswordAsync(admin, resetToken, options.AdminPassword);

        if (!resetResult.Succeeded)
        {
            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            logger.LogWarning("Failed to reset admin password for {Email}: {Errors}", options.AdminEmail, errors);
        }
        else
        {
            logger.LogInformation("Ensured admin credentials for {Email}", options.AdminEmail);
        }

        if (!await userManager.IsInRoleAsync(admin, SystemRoles.SystemAdmin))
        {
            await userManager.AddToRoleAsync(admin, SystemRoles.SystemAdmin);
        }
    }
}
