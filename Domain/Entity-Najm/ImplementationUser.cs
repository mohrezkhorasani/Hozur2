using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class ImplementationUser
{
    public int Id { get; set; }

    public string? Firstname { get; set; }

    public string? Lastname { get; set; }

    public string? User { get; set; }

    public string? ShHesab { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Implementation> Implementations { get; set; } = new List<Implementation>();
}
