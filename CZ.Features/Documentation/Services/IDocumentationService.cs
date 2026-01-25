namespace api.CZ.Features.Documentation.Services;

public interface IDocumentationService
{
    Task<string?> GetDocumentationAsHtmlAsync(string[] pathSegments);
}