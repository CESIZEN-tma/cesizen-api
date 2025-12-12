namespace api.CZ.Data.AbstractModels;

public abstract class SoftDeletableEntity : BaseEntity
{
    public DateTime? DeletionTime { get; set; }
}