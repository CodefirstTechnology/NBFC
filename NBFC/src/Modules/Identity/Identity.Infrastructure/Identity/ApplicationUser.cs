using Microsoft.AspNetCore.Identity;
using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.Modules.Identity.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>, IAuditableEntity, ISoftDeletable, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid? BranchId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}
