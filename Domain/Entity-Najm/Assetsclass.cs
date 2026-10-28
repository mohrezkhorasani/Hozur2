using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Assetsclass
{
    public int Id { get; set; }

    public int? Classification { get; set; }

    public string? ClassName { get; set; }

    public double? Rate { get; set; }

    public int? UsefulLife { get; set; }

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
