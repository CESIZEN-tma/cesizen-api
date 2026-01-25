namespace api.CZ.Data.AbstractModels;

public abstract class BaseEntity
{
    public Guid? Id { get; set; }
    public DateTime? CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}