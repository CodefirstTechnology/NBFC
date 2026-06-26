namespace Patsanstha.BuildingBlocks.Domain.Abstractions;

public interface IAuditableEntity
{
    Guid? CreatedBy { get; set; }

    DateTimeOffset CreatedAt { get; set; }

    Guid? ModifiedBy { get; set; }

    DateTimeOffset? ModifiedAt { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

public interface ITenantScoped
{
    Guid TenantId { get; set; }
}

public interface IConcurrencyTracked
{
    byte[] RowVersion { get; set; }
}
