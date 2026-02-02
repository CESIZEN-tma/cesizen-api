using api.CZ.Features.InformationTags.DTOs;
using api.CZ.Features.InformationTags.Models;

namespace api.CZ.Features.InformationTags.Extensions;

public static class InformationTagExtensions
{
    public static GetInformationTagDto ToDto(this InformationTag tag)
    {
        return new GetInformationTagDto
        {
            Id = tag.Id,
            Label = tag.Label,
            CreationTime = tag.CreationTime,
            UpdateTime = tag.UpdateTime
        };
    }
}
