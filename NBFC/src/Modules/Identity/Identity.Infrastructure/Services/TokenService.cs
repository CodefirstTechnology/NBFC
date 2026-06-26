using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Identity.Application.Abstractions;
using Patsanstha.Modules.Identity.Domain.Authorization;
using Patsanstha.Modules.Identity.Domain.Entities;
using Patsanstha.Modules.Identity.Infrastructure.Identity;
using Patsanstha.Modules.Identity.Infrastructure.Options;
using Patsanstha.Modules.Identity.Infrastructure.Persistence;

namespace Patsanstha.Modules.Identity.Infrastructure.Services;

public sealed class TokenService(
    IdentityDbContext dbContext,
    IOptions<JwtOptions> jwtOptions) : ITokenService
{
    private readonly JwtOptions _options = jwtOptions.Value;

    public async Task<Result<TokenPairResponse>> IssueTokensAsync(
        Guid userId,
        string email,
        string fullName,
        Guid tenantId,
        Guid? branchId,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var accessExpires = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var refreshExpires = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenDays);

        var accessToken = BuildAccessToken(userId, email, fullName, tenantId, branchId, roles, permissions, accessExpires);
        var refreshTokenPlain = GenerateSecureToken();
        var refreshTokenHash = HashToken(refreshTokenPlain);

        dbContext.RefreshTokens.Add(RefreshToken.Create(
            userId,
            tenantId,
            refreshTokenHash,
            refreshExpires,
            ipAddress));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new TokenPairResponse(
            accessToken,
            accessExpires,
            refreshTokenPlain,
            refreshExpires));
    }

    public async Task<Result<TokenPairResponse>> RotateRefreshTokenAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return Result.Failure<TokenPairResponse>(
                Error.Unauthorized("Auth.RefreshToken.Invalid", "Refresh token is invalid or expired."));
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == storedToken.UserId && u.IsActive, cancellationToken);

        if (user is null)
        {
            return Result.Failure<TokenPairResponse>(
                Error.Unauthorized("Auth.User.Inactive", "User account is inactive."));
        }

        var roles = await GetUserRolesAsync(user.Id, cancellationToken);
        var permissions = await GetUserPermissionsAsync(user.Id, cancellationToken);

        var accessExpires = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var refreshExpires = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenDays);
        var newRefreshPlain = GenerateSecureToken();
        var newRefreshHash = HashToken(newRefreshPlain);

        storedToken.Revoke(newRefreshHash);

        var accessToken = BuildAccessToken(
            user.Id,
            user.Email!,
            user.FullName,
            user.TenantId,
            user.BranchId,
            roles,
            permissions,
            accessExpires);

        dbContext.RefreshTokens.Add(RefreshToken.Create(
            user.Id,
            user.TenantId,
            newRefreshHash,
            refreshExpires,
            ipAddress));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new TokenPairResponse(
            accessToken,
            accessExpires,
            newRefreshPlain,
            refreshExpires));
    }

    public async Task<Result> RevokeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
        {
            return Result.Success();
        }

        if (storedToken.IsActive)
        {
            storedToken.Revoke();
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    private string BuildAccessToken(
        Guid userId,
        string email,
        string fullName,
        Guid tenantId,
        Guid? branchId,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        DateTimeOffset expiresAt)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("name", fullName),
            new("tenant_id", tenantId.ToString()),
        };

        if (branchId.HasValue)
        {
            claims.Add(new Claim("branch_id", branchId.Value.ToString()));
        }

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim(Permissions.ClaimType, p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<IReadOnlyList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await (
            from userRole in dbContext.UserRoles
            join role in dbContext.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            select role.Name!
        ).ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var roleIds = await dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        return await dbContext.RoleClaims
            .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == Permissions.ClaimType)
            .Select(rc => rc.ClaimValue!)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
