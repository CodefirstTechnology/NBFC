namespace Patsanstha.Modules.Identity.Domain.Authorization;

/// <summary>
/// Permission matrix — policy names map 1:1 to these constants.
/// Stored as role claims: claim type = "permission", claim value = permission code.
/// </summary>
public static class Permissions
{
    public const string ClaimType = "permission";

    // Members
    public const string MembersRead = "members.read";
    public const string MembersCreate = "members.create";
    public const string MembersUpdate = "members.update";
    public const string MembersDelete = "members.delete";

    // Deposits
    public const string DepositsRead = "deposits.read";
    public const string DepositsCreate = "deposits.create";
    public const string DepositsUpdate = "deposits.update";

    // Loans
    public const string LoansRead = "loans.read";
    public const string LoansCreate = "loans.create";
    public const string LoansApprove = "loans.approve";
    public const string LoansDisburse = "loans.disburse";

    // Collections
    public const string CollectionsRead = "collections.read";
    public const string CollectionsCollect = "collections.collect";

    // Recovery
    public const string RecoveryRead = "recovery.read";
    public const string RecoveryManage = "recovery.manage";

    // Accounting
    public const string AccountingRead = "accounting.read";
    public const string AccountingPost = "accounting.post";

    // Reporting
    public const string ReportsRead = "reports.read";
    public const string ReportsExport = "reports.export";

    // Admin
    public const string AdminUsersManage = "admin.users.manage";
    public const string AdminRolesManage = "admin.roles.manage";
    public const string AdminAuditRead = "admin.audit.read";
    public const string AdminSettingsManage = "admin.settings.manage";

    public static IReadOnlyList<string> All { get; } =
    [
        MembersRead, MembersCreate, MembersUpdate, MembersDelete,
        DepositsRead, DepositsCreate, DepositsUpdate,
        LoansRead, LoansCreate, LoansApprove, LoansDisburse,
        CollectionsRead, CollectionsCollect,
        RecoveryRead, RecoveryManage,
        AccountingRead, AccountingPost,
        ReportsRead, ReportsExport,
        AdminUsersManage, AdminRolesManage, AdminAuditRead, AdminSettingsManage,
    ];
}
