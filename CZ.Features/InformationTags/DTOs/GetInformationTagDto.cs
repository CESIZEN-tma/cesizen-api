namespace api.CZ.Features.InformationTags.DTOs;

public class GetInformationTagDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}
