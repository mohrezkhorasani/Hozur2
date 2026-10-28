using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Asset
{
    public int Id { get; set; }

    public int Primassetno { get; set; }

    public int? Secenassetno { get; set; }

    public string Name { get; set; } = null!;

    public int? Place { get; set; }

    public string? UserName { get; set; }

    public DateOnly? BuyDate { get; set; }

    public DateOnly? EsghatDate { get; set; }

    public int? State { get; set; }

    public int? Regionid { get; set; }

    public string? EsghatNote { get; set; }

    public int? Unit { get; set; }

    public double? Price { get; set; }

    public string? Description { get; set; }

    public string? Docno { get; set; }

    public double? Depreciation { get; set; }

    public int? ClassId { get; set; }

    public double? Rate { get; set; }

    public int? UsefulLife { get; set; }

    public virtual Assetsclass? Class { get; set; }

    public virtual Unit? PlaceNavigation { get; set; }

    public virtual Region? Region { get; set; }

    public virtual Unit? UnitNavigation { get; set; }
}
