using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class SalaryUserPayGroup
{
    public int Id { get; set; }

    public int? Bid { get; set; }

    public string? Name { get; set; }

    public double? MablaghJ { get; set; }

    public double? MablaghSaat { get; set; }

    public int? Month { get; set; }

    public int? Year { get; set; }

    public virtual SalaryUserPayBase? BidNavigation { get; set; }
}
