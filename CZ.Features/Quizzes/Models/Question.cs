namespace api.CZ.Features.Quizzes.Models;

public partial class Question
{
    public Guid Id { get; set; }

    public string Text { get; set; } = null!;

    public int Position { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public Guid IdQuizz { get; set; }

    public virtual Quizz IdQuizzNavigation { get; set; } = null!;

    public virtual ICollection<ResponsesOption> ResponsesOptions { get; set; } = new List<ResponsesOption>();
}
