using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class FundUsersPaymentBase
{
    public int Id { get; set; }

    public int? Uid { get; set; }

    public int? Fub { get; set; }

    public int? LoanNumber { get; set; }

    public double? TotalFee { get; set; }

    public int? InstallmentNumber { get; set; }

    public double? InstallmentFee { get; set; }

    public int? RemainedNumber { get; set; }

    public DateOnly? PayDate { get; set; }

    public double? InstallmentFeeFirst { get; set; }

    public double? InstallmentFeeLast { get; set; }

    public bool? PayFirst { get; set; }

    public bool? PayLast { get; set; }

    public bool? SalaryFraction { get; set; }

    public bool? Paid { get; set; }

    public int? FundType { get; set; }

    public virtual FundUsersBase? FubNavigation { get; set; }

    public virtual ICollection<FundUsersPay> FundUsersPays { get; set; } = new List<FundUsersPay>();
}
