namespace WebAPI.Data;

public abstract class BaseEntityWithId<TypeId> : BaseEntity where TypeId : struct
{
    public TypeId Id { get; set; }
}

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}