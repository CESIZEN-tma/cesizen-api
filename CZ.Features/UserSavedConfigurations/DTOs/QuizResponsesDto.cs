namespace api.CZ.Features.UserSavedConfigurations.DTOs;

public class QuizResponsesDto
{
    public Guid QuizId { get; set; }
    public List<QuestionResponseDto> Responses { get; set; } = new();
}

public class QuestionResponseDto
{
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
}
