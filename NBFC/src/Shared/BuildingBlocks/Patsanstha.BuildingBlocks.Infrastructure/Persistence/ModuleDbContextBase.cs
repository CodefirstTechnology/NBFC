using Microsoft.EntityFrameworkCore;
using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;
using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Audit;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;

namespace Patsanstha.BuildingBlocks.Infrastructure.Persistence;

public abstract class ModuleDbContextBase : DbContext, IOutboxDbContext
{
    private readonly IAuditContextAccessor _auditContextAccessor;

    protected ModuleDbContextBase(
        DbContextOptions options,
        IAuditContextAccessor auditContextAccessor)
        : base(options)
    {
        _auditContextAccessor = auditContextAccessor;
    }

    protected abstract string SchemaName { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        OutboxModelConfiguration.ConfigureOutbox(modelBuilder, SchemaName);
        AuditLogModelConfiguration.ConfigureAuditLog(modelBuilder, SchemaName);
        ConfigureModule(modelBuilder);
        ApplyGlobalFilters(modelBuilder);
    }

    protected abstract void ConfigureModule(ModelBuilder modelBuilder);

    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ModuleDbContextBase)
                    .GetMethod(nameof(ApplySoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(this, [modelBuilder]);
            }

            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ModuleDbContextBase)
                    .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(this, [modelBuilder]);
            }
        }
    }

    private void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDeletable
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == _auditContextAccessor.Current.TenantId);
    }

    protected void ConfigureAuditableEntity<TEntity>(ModelBuilder modelBuilder)
        where TEntity : AuditableEntity
    {
        modelBuilder.Entity<TEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RowVersion).ConfigureRowVersion();
            entity.HasIndex(e => new { e.TenantId, e.IsDeleted });
        });
    }

    async Task<IReadOnlyList<OutboxMessageEntity>> IOutboxDbContext.GetPendingOutboxMessagesAsync(
        CancellationToken cancellationToken) =>
        await OutboxDbContextExtensions.GetPendingOutboxMessagesAsync(this, cancellationToken: cancellationToken);

    Task IOutboxDbContext.SaveChangesAsync(CancellationToken cancellationToken) =>
        base.SaveChangesAsync(cancellationToken);
}
