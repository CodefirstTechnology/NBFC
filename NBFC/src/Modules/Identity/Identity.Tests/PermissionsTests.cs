using Patsanstha.Modules.Identity.Domain.Authorization;

namespace Patsanstha.Modules.Identity.Tests;

public sealed class PermissionsTests
{
    [Fact]
    public void All_permissions_are_unique()
    {
        var duplicates = Permissions.All
            .GroupBy(p => p)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void Every_default_role_permission_is_registered()
    {
        foreach (var permission in SystemRoles.DefaultRolePermissions.Values.SelectMany(p => p))
        {
            Assert.Contains(permission, Permissions.All);
        }
    }
}

public sealed class TwoFactorPolicyTests
{
    [Theory]
    [InlineData(SystemRoles.Chairman, true)]
    [InlineData(SystemRoles.BranchManager, true)]
    [InlineData(SystemRoles.SystemAdmin, true)]
    [InlineData(SystemRoles.Teller, false)]
    [InlineData(SystemRoles.LoanOfficer, false)]
    public void Eligible_roles_are_enforced(string role, bool expected)
    {
        Assert.Equal(expected, TwoFactorPolicy.IsEligibleRole(role));
    }
}

public sealed class TokenHashingTests
{
    [Fact]
    public void HashToken_is_deterministic()
    {
        var hash1 = Patsanstha.Modules.Identity.Infrastructure.Services.TokenService.HashToken("sample-token");
        var hash2 = Patsanstha.Modules.Identity.Infrastructure.Services.TokenService.HashToken("sample-token");

        Assert.Equal(hash1, hash2);
        Assert.NotEqual("sample-token", hash1);
    }
}
