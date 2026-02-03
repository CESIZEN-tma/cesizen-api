using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.Quizzes.DTOs;

public class CreateQuizzDto
{
    [Required]
    [StringLength(255)]
    public required string Nom { get; set; }

    public bool Active { get; set; } = true;

    public List<CreateQuestionDto> Questions { get; set; } = new();
}

public class CreateQuestionDto
{
    [Required]
    public required string Text { get; set; }

    [Range(0, int.MaxValue)]
    public int Position { get; set; }

    public List<CreateResponseOptionDto> Options { get; set; } = new();
}

public class CreateResponseOptionDto
{
    [Required]
    public required string Label { get; set; }

    [Range(0, int.MaxValue)]
    public int Position { get; set; }

    [Required]
    public required string TargetedField { get; set; }

    [Required]
    public required string Operation { get; set; }

    [Required]
    public required string Value { get; set; }
}
