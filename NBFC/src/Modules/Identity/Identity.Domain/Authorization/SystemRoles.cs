namespace Patsanstha.Modules.Identity.Domain.Authorization;

public static class SystemRoles
{
    public const string Chairman = "Chairman";
    public const string BranchManager = "BranchManager";
    public const string Teller = "Teller";
    public const string LoanOfficer = "LoanOfficer";
    public const string RecoveryAgent = "RecoveryAgent";
    public const string Accountant = "Accountant";
    public const string SystemAdmin = "SystemAdmin";

    public static IReadOnlyList<string> All { get; } =
    [
        Chairman, BranchManager, Teller, LoanOfficer,
        RecoveryAgent, Accountant, SystemAdmin,
    ];

    /// <summary>
    /// Default permission assignments per role for seeding.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> DefaultRolePermissions { get; } =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [SystemAdmin] = Permissions.All,
            [Chairman] =
            [
                Permissions.MembersRead, Permissions.DepositsRead, Permissions.LoansRead,
                Permissions.LoansApprove, Permissions.CollectionsRead, Permissions.RecoveryRead,
                Permissions.AccountingRead, Permissions.ReportsRead, Permissions.ReportsExport,
                Permissions.AdminAuditRead,
            ],
            [BranchManager] =
            [
                Permissions.MembersRead, Permissions.MembersCreate, Permissions.MembersUpdate,
                Permissions.DepositsRead, Permissions.DepositsCreate,
                Permissions.LoansRead, Permissions.LoansCreate, Permissions.LoansApprove, Permissions.LoansDisburse,
                Permissions.CollectionsRead, Permissions.CollectionsCollect,
                Permissions.RecoveryRead, Permissions.RecoveryManage,
                Permissions.AccountingRead, Permissions.ReportsRead,
                Permissions.AdminUsersManage,
            ],
            [Teller] =
            [
                Permissions.MembersRead,
                Permissions.DepositsRead, Permissions.DepositsCreate,
                Permissions.CollectionsRead, Permissions.CollectionsCollect,
            ],
            [LoanOfficer] =
            [
                Permissions.MembersRead, Permissions.MembersCreate,
                Permissions.LoansRead, Permissions.LoansCreate,
                Permissions.DepositsRead,
            ],
            [RecoveryAgent] =
            [
                Permissions.MembersRead,
                Permissions.LoansRead,
                Permissions.CollectionsRead,
                Permissions.RecoveryRead, Permissions.RecoveryManage,
            ],
            [Accountant] =
            [
                Permissions.MembersRead, Permissions.DepositsRead, Permissions.LoansRead,
                Permissions.CollectionsRead, Permissions.AccountingRead, Permissions.AccountingPost,
                Permissions.ReportsRead, Permissions.ReportsExport,
            ],
        };
}
