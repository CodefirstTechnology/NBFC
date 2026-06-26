namespace Patsanstha.Modules.Identity.Domain.Authorization;

public static class TwoFactorPolicy
{
    public static IReadOnlyList<string> EligibleRoles { get; } =
    [
        SystemRoles.Chairman,
        SystemRoles.BranchManager,
        SystemRoles.SystemAdmin,
    ];

    public static bool IsEligibleRole(string roleName) =>
        EligibleRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
}
