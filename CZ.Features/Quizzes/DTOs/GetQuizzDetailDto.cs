namespace api.CZ.Features.Quizzes.DTOs;

public class GetQuizzDetailDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = null!;
    public bool Active { get; set; }
    public DateTime CreationTime { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
}

public class QuestionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public int Position { get; set; }
    public List<ResponseOptionDto> Options { get; set; } = new();
}

public class ResponseOptionDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = null!;
    public int Position { get; set; }
    public string TargetedField { get; set; } = null!;
    public string Operation { get; set; } = null!;
    public string Value { get; set; } = null!;
}
