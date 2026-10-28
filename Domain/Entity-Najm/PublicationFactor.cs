using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationFactor
{
    public int Id { get; set; }

    public DateOnly? Date { get; set; }

    public string? Buyer { get; set; }

    public int? Invoicenumber { get; set; }

    public string? Description { get; set; }

    public bool? Istemp { get; set; }

    public double? TotalPrices { get; set; }

    public double? Discount { get; set; }

    public string? Phonenumber { get; set; }

    public double? CdDiscount { get; set; }

    public double? Discountper { get; set; }

    public double? CdDiscountper { get; set; }

    public double? Payment { get; set; }

    public double? Remaining { get; set; }

    public bool? Paid { get; set; }

    public DateOnly? PayDate { get; set; }

    public virtual ICollection<PublicationFactorItem> PublicationFactorItems { get; set; } = new List<PublicationFactorItem>();
}
