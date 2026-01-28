namespace api.CZ.Features.Quizzes.Models;

public partial class ResponsesOption
{
    public Guid Id { get; set; }

    public string Label { get; set; } = null!;

    public int Position { get; set; }

    public string TargetedField { get; set; } = null!;

    public string Operation { get; set; } = null!;

    public string Value { get; set; } = null!;

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public Guid IdQuestions { get; set; }

    public virtual Question IdQuestionsNavigation { get; set; } = null!;
}
