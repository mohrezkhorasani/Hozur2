using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class CostsCode
{
    public int Id { get; set; }

    public string? CostName { get; set; }

    public virtual ICollection<TAccFactor> TAccFactors { get; set; } = new List<TAccFactor>();
}
