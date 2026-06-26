using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence;
using Patsanstha.Modules.Members.Application.Abstractions;
using Patsanstha.Modules.Members.Domain.Entities;
using Patsanstha.Modules.Members.Domain.Enums;

namespace Patsanstha.Modules.Members.Infrastructure.Persistence;

public sealed class MembersDbContext(
    DbContextOptions<MembersDbContext> options,
    IAuditContextAccessor auditContextAccessor)
    : ModuleDbContextBase(options, auditContextAccessor)
{
    protected override string SchemaName => Schema;

    public const string Schema = "members";

    public DbSet<Member> Members => Set<Member>();

    protected override void ConfigureModule(ModelBuilder modelBuilder)
    {
        ConfigureAuditableEntity<Member>(modelBuilder);

        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("members");

            entity.Property(m => m.MemberNumber).HasMaxLength(32).IsRequired();
            entity.Property(m => m.FullName).HasMaxLength(200).IsRequired();
            entity.Property(m => m.Gender).HasMaxLength(20).IsRequired();
            entity.Property(m => m.MobileNumber).HasMaxLength(15).IsRequired();
            entity.Property(m => m.Email).HasMaxLength(256);
            entity.Property(m => m.AddressLine1).HasMaxLength(300).IsRequired();
            entity.Property(m => m.AddressLine2).HasMaxLength(300);
            entity.Property(m => m.City).HasMaxLength(100).IsRequired();
            entity.Property(m => m.State).HasMaxLength(100).IsRequired();
            entity.Property(m => m.PinCode).HasMaxLength(10).IsRequired();
            entity.Property(m => m.AadhaarEncrypted).HasMaxLength(512).IsRequired();
            entity.Property(m => m.PanEncrypted).HasMaxLength(512).IsRequired();
            entity.Property(m => m.AadhaarHash).HasMaxLength(128).IsRequired();
            entity.Property(m => m.NomineeName).HasMaxLength(200);
            entity.Property(m => m.NomineeRelation).HasMaxLength(100);
            entity.Property(m => m.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

            entity.HasIndex(m => new { m.TenantId, m.MemberNumber })
                .IsUnique()
                .HasDatabaseName("ux_members_tenant_member_number");

            entity.HasIndex(m => new { m.TenantId, m.AadhaarHash })
                .IsUnique()
                .HasDatabaseName("ux_members_tenant_aadhaar_hash");

            entity.HasIndex(m => new { m.TenantId, m.BranchId, m.Status })
                .HasDatabaseName("ix_members_tenant_branch_status");

            entity.HasIndex(m => new { m.TenantId, m.FullName })
                .HasDatabaseName("ix_members_tenant_full_name");

            entity.ToTable(t => t.HasCheckConstraint(
                "ck_members_status",
                "\"Status\" IN ('Pending','Active','Inactive','Suspended','Closed')"));
        });
    }
}

public sealed class MemberRepository(MembersDbContext dbContext) : IMemberRepository
{
    public Task AddAsync(Member member, CancellationToken cancellationToken = default)
    {
        dbContext.Members.Add(member);
        return Task.CompletedTask;
    }

    public Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Members.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<bool> ExistsByAadhaarHashAsync(string aadhaarHash, CancellationToken cancellationToken = default) =>
        dbContext.Members.AnyAsync(m => m.AadhaarHash == aadhaarHash, cancellationToken);

    public async Task<(IReadOnlyList<Member> Items, int TotalCount)> ListAsync(
        ListMembersCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Members.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var search = criteria.Search.Trim().ToLowerInvariant();
            query = query.Where(m =>
                m.FullName.ToLower().Contains(search) ||
                m.MemberNumber.ToLower().Contains(search) ||
                m.MobileNumber.Contains(search));
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(m => m.Status == criteria.Status.Value);
        }

        if (criteria.BranchId.HasValue)
        {
            query = query.Where(m => m.BranchId == criteria.BranchId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

public sealed class MemberNumberGenerator(
    MembersDbContext dbContext,
    IAuditContextAccessor auditContextAccessor) : IMemberNumberGenerator
{
    public async Task<string> GenerateNextAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = auditContextAccessor.Current.TenantId;
        var year = DateTime.UtcNow.Year;
        var prefix = $"M{year}";

        var latestNumber = await dbContext.Members
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.MemberNumber.StartsWith(prefix))
            .OrderByDescending(m => m.MemberNumber)
            .Select(m => m.MemberNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = 1;

        if (latestNumber is not null && latestNumber.Length > prefix.Length)
        {
            _ = int.TryParse(latestNumber[prefix.Length..], out sequence);
            sequence++;
        }

        return $"{prefix}{sequence:D5}";
    }
}

public sealed class MemberMapper(IPiiEncryptionService piiEncryption) : IMemberMapper
{
    public MemberSummaryDto ToSummary(Member member) =>
        new(
            member.Id,
            member.MemberNumber,
            member.FullName,
            member.MobileNumber,
            member.BranchId,
            member.Status,
            member.JoinedOn);

    public MemberDetailDto ToDetail(Member member)
    {
        var aadhaarPlain = piiEncryption.Decrypt(member.AadhaarEncrypted);
        var panPlain = piiEncryption.Decrypt(member.PanEncrypted);

        return new MemberDetailDto(
            member.Id,
            member.MemberNumber,
            member.FullName,
            member.DateOfBirth,
            member.Gender,
            member.MobileNumber,
            member.Email,
            member.AddressLine1,
            member.AddressLine2,
            member.City,
            member.State,
            member.PinCode,
            piiEncryption.MaskAadhaar(aadhaarPlain),
            piiEncryption.MaskPan(panPlain),
            member.NomineeName,
            member.NomineeRelation,
            member.BranchId,
            member.Status,
            member.JoinedOn,
            member.CreatedAt,
            member.ModifiedAt);
    }
}
