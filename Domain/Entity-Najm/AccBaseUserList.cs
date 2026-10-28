using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class AccBaseUserList
{
    public int Id { get; set; }

    public int? Uid { get; set; }

    public int? Bid { get; set; }

    public int? Pid { get; set; }

    public int? SaatKarkard { get; set; }

    public int? SaatEzafekari { get; set; }

    public int? SaatMamoriat { get; set; }

    public double? MablaghBoan { get; set; }

    public double? MablaghMaskan { get; set; }

    public double? MablaghMamoriat { get; set; }

    public double? MablaghEzafehkar { get; set; }

    public double? MablaghKarkard { get; set; }

    public double? MablaghHghogh { get; set; }

    public double? MablaghKsorat { get; set; }

    public double? MablaghBimeh { get; set; }

    public double? MablaghBimehTakmili { get; set; }

    public double? MablaghMaliat { get; set; }

    public double? MablaghOlad { get; set; }

    public bool? Locked { get; set; }

    public int? Year { get; set; }

    public int? Month { get; set; }

    public double? MablaghSandogh { get; set; }

    public double? MablaghBestankari { get; set; }

    public double? MablaghBedehkari { get; set; }

    public double? Karaneh { get; set; }

    public int? SaatTashvighi { get; set; }

    public double? MablaghTashvighi { get; set; }

    public string? Decription { get; set; }

    public string? Pdate { get; set; }

    public string? Datefrom { get; set; }

    public string? Dateto { get; set; }

    public double? FundLoanPay { get; set; }

    public double? FundMonthly { get; set; }

    public int? ModirPercent { get; set; }

    public double? MablaghModirPercent { get; set; }

    public double? MablaghKhalesPardakhti { get; set; }

    public double? MablaghBimehKarfarma { get; set; }

    public double? MablaghBimehTakmiliKarfarma { get; set; }

    public double? MablaghOther { get; set; }

    public double? MablaghBimehKasr { get; set; }

    public double? MablaghMasouliat { get; set; }

    public double? MablaghTaahol { get; set; }

    public virtual AccBaseUser? BidNavigation { get; set; }

    public virtual AccPardakht? PidNavigation { get; set; }

    public virtual TUser? UidNavigation { get; set; }
}
