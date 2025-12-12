using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;

namespace api.scaffold;

public partial class Question : SoftDeletableEntity
{

    public string Text { get; set; } = null!;

    public int Position { get; set; }
    
    public Guid IdQuizz { get; set; }

    public virtual Quizz IdQuizzNavigation { get; set; } = null!;

    public virtual ICollection<ResponsesOption> ResponsesOptions { get; set; } = new List<ResponsesOption>();
}
