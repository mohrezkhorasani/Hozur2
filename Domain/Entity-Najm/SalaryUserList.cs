using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class SalaryUserList
{
    public int Id { get; set; }

    public int Uid { get; set; }

    public int Bid { get; set; }

    public int? Pid { get; set; }

    public double? Bmablagh { get; set; }

    public double? SaatKari { get; set; }

    public double? SaatEzafekari { get; set; }

    public double? SaatMamoriat { get; set; }

    public double? SaatPadash { get; set; }

    public double? MablaghBimeh { get; set; }

    public double? MablaghBestankari { get; set; }

    public double? Maliat { get; set; }

    public bool? KasrBimeh { get; set; }

    public double? MablaghNahaee { get; set; }

    public double? Khales { get; set; }

    public string? Datefrom { get; set; }

    public string? DateTo { get; set; }

    public string? Pdate { get; set; }

    public int? Month { get; set; }

    public int? Year { get; set; }

    public string? Describ { get; set; }

    public bool? Islock { get; set; }

    public double? FundMonthly { get; set; }

    public double? FundLoanPay { get; set; }

    public double? MablaghBedehkari { get; set; }

    public bool? ModirePercent { get; set; }

    public DateOnly? Bimdatestar { get; set; }

    public DateOnly? Bimdateend { get; set; }

    public double? MablaghBimehC { get; set; }

    public double? Other { get; set; }

    public double? MablaghBimehCHekmat { get; set; }

    public double? MablaghBimehHekmat { get; set; }

    public double? BimehCHekmat { get; set; }

    public double? Hagholad { get; set; }

    public double? Refahi { get; set; }

    public double? Maskan { get; set; }

    public double? Boan { get; set; }

    public int? TedadOlad { get; set; }

    public int? IncomYear { get; set; }

    public int? ActiveYear { get; set; }

    public virtual SalaryUserBase BidNavigation { get; set; } = null!;

    public virtual SalaryPardakht? PidNavigation { get; set; }

    public virtual TUser UidNavigation { get; set; } = null!;
}
