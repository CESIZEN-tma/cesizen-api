using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;

namespace api.scaffold;

public partial class ResponsesOption : SoftDeletableEntity
{

    public string Label { get; set; } = null!;

    public int Position { get; set; }

    public string TargetedField { get; set; } = null!;

    public string Operation { get; set; } = null!;

    public string Value { get; set; } = null!;
    
    public Guid IdQuestions { get; set; }

    public virtual Question IdQuestionsNavigation { get; set; } = null!;
}
