namespace api.CZ.Features.Quizzes.DTOs;

public class GetQuizzDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = null!;
    public bool Active { get; set; }
    public DateTime CreationTime { get; set; }
    public int QuestionCount { get; set; }
}
