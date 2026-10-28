using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationCost
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public double? Cost { get; set; }

    public int? Percentage { get; set; }

    public int? CostType { get; set; }

    public string? Description { get; set; }

    public double? TotalCost { get; set; }

    public int? Published { get; set; }

    public virtual PublicationsRegisterBook Book { get; set; } = null!;
}
