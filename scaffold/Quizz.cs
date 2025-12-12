using System;
using System.Collections.Generic;
using api.CZ.Data.AbstractModels;

namespace api.scaffold;

public partial class Quizz : SoftDeletableEntity
{

    public string Nom { get; set; } = null!;

    public bool Active { get; set; }
    
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
