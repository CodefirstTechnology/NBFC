namespace Patsanstha.Modules.Identity.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Patsanstha";

    public string Audience { get; set; } = "Patsanstha.Api";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 7;
}

public sealed class IdentitySeedOptions
{
    public const string SectionName = "IdentitySeed";

    public Guid DefaultTenantId { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public string AdminEmail { get; set; } = "admin@patsanstha.local";

    public string AdminPassword { get; set; } = "ChangeMe@123";

    public string AdminFullName { get; set; } = "System Administrator";
}
