namespace api.CZ.Data.AbstractModels;

public abstract class BaseEntity
{
    public required Guid Id { get; set; }
    public required DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}