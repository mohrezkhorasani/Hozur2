using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class FundUsersBase
{
    public int Id { get; set; }

    public int Uid { get; set; }

    public int? Fid { get; set; }

    public DateOnly? StartDate { get; set; }

    public double? StartFee { get; set; }

    public double? MonthlyFee { get; set; }

    public int? PaymentNumber { get; set; }

    public double? PaymentFee { get; set; }

    public double? PaymentTotal { get; set; }

    public bool? SalaryFraction { get; set; }

    public bool? Paid { get; set; }

    public int Region { get; set; }

    public virtual ICollection<FundUsersPaymentBase> FundUsersPaymentBases { get; set; } = new List<FundUsersPaymentBase>();

    public virtual ICollection<FundUsersPay> FundUsersPays { get; set; } = new List<FundUsersPay>();

    public virtual Region RegionNavigation { get; set; } = null!;
}
