using api.CZ.Features.InformationPages.DTOs;
using api.CZ.Features.InformationPages.Models;

namespace api.CZ.Features.InformationPages.Extensions;

public static class InformationPageExtensions
{
    public static GetInformationPageDto ToDto(this InformationPage page)
    {
        return new GetInformationPageDto
        {
            Id = page.Id,
            Title = page.Title,
            Description = page.Description,
            Content = page.Content,
            ContentType = page.ContentType,
            CurrentlyEditing = page.CurrentlyEditing,
            Status = page.Status,
            Active = page.Active,
            CreationTime = page.CreationTime,
            UpdateTime = page.UpdateTime,
            IdAdministrators = page.IdAdministrators
        };
    }
}
