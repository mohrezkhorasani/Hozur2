using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationFactorItem
{
    public int Id { get; set; }

    public int? FactorId { get; set; }

    public int? Bookid { get; set; }

    public int? Numbers { get; set; }

    public double? Price { get; set; }

    public double? Discount { get; set; }

    public double? TotalPrice { get; set; }

    public int? Booktype { get; set; }

    public virtual PublicationsRegisterBook? Book { get; set; }

    public virtual PublicationFactor? Factor { get; set; }
}
