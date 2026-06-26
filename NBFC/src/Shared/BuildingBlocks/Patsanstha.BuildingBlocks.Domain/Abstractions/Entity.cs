namespace Patsanstha.BuildingBlocks.Domain.Abstractions;

public abstract class Entity
{
    public Guid Id { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

public abstract class AuditableEntity : Entity, IAuditableEntity, ISoftDeletable, ITenantScoped, IConcurrencyTracked
{
    public Guid TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = [];
}

public abstract class AggregateRoot : AuditableEntity;
