using System;
using System.Collections.Generic;

namespace api.scaffoldBis;

public partial class Quizz
{
    public Guid Id { get; set; }

    public string Nom { get; set; } = null!;

    public bool Active { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime? DeletionTime { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
