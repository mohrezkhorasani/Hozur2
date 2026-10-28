using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TAccFactor
{
    public int Id { get; set; }

    public int? SId { get; set; }

    public string? Sharh { get; set; }

    public DateOnly? Date { get; set; }

    public double? Mablagh { get; set; }

    public string? Describ { get; set; }

    public string? Tempdate1 { get; set; }

    public string? Tempdate2 { get; set; }

    public int? Type { get; set; }

    public string? Pdate { get; set; }

    public int? Regionid { get; set; }

    public int? CostCode { get; set; }

    public virtual CostsCode? CostCodeNavigation { get; set; }

    public virtual Region? Region { get; set; }
}
