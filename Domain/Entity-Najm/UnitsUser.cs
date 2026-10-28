using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class UnitsUser
{
    public int Id { get; set; }

    public int Uid { get; set; }

    public int Unitid { get; set; }

    public virtual Unit Unit { get; set; } = null!;
}
