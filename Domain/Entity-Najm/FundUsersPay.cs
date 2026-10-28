using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class FundUsersPay
{
    public int Id { get; set; }

    public int? Uid { get; set; }

    public int? Fub { get; set; }

    public int? Fupb { get; set; }

    public int? Year { get; set; }

    public int? Month { get; set; }

    public double? Fee { get; set; }

    public int? Paytype { get; set; }

    public bool? SalaryFraction { get; set; }

    public bool? Paid { get; set; }

    public int? PayMethod { get; set; }

    public DateOnly? PayDate { get; set; }

    public string? Description { get; set; }

    public virtual FundUsersBase? FubNavigation { get; set; }

    public virtual FundUsersPaymentBase? FupbNavigation { get; set; }
}
