using System.ComponentModel.DataAnnotations;

namespace api.CZ.Features.Quizzes.DTOs;

public class UpdateQuizzDto
{
    [Required]
    [StringLength(255)]
    public required string Nom { get; set; }

    public bool Active { get; set; }
}
