using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Implementationsubject
{
    public int Id { get; set; }

    public string? Subject { get; set; }

    public virtual ICollection<Implementation> Implementations { get; set; } = new List<Implementation>();
}
