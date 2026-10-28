using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TAccBase
{
    public int Id { get; set; }

    public int Year { get; set; }

    public int? Base1 { get; set; }

    public int? Base2 { get; set; }

    public int? Base3 { get; set; }

    public int? Base4 { get; set; }

    public int? Base5 { get; set; }

    public int? Base6 { get; set; }

    public int? Base7 { get; set; }

    public int? Base8 { get; set; }

    public int? Base9 { get; set; }

    public int? Base10 { get; set; }

    public int? Base11 { get; set; }

    public int? Base12 { get; set; }

    public int? Base13 { get; set; }

    public int? Base14 { get; set; }

    public int? Base15 { get; set; }

    public int? Base16 { get; set; }

    public int? Base17 { get; set; }

    public int? Base18 { get; set; }

    public int? Base19 { get; set; }

    public int? Base20 { get; set; }

    public int? Base21 { get; set; }

    public int? Bsabegheh1 { get; set; }

    public int? Bsabegheh2 { get; set; }

    public int? Bsabegheh3 { get; set; }

    public int? Bmodiriat { get; set; }

    public int? Btaahol { get; set; }

    public int? Btahsilat { get; set; }

    public int? Bmasoliat { get; set; }

    public int? SaghfMaliat { get; set; }

    public int? Maliat { get; set; }

    public int? Ezafehpercent { get; set; }

    public int? Ezafehsaghf { get; set; }

    public int? Codeshobeh { get; set; }

    public int? Codesazman { get; set; }

    public int? Masooliatmozaf { get; set; }

    public string? LastStep { get; set; }

    public DateOnly? LastDate { get; set; }

    /// <summary>
    /// Locking
    /// </summary>
    public bool? Locked { get; set; }

    public int? MinimumMonthlyFee { get; set; }

    public string? FundAccount { get; set; }

    public string? FundBank { get; set; }

    public string? FundName { get; set; }

    public string? FundAlias { get; set; }

    public int? MinimumStartFee { get; set; }

    public double? Mahaneh { get; set; }

    public double? Tatilat { get; set; }

    public int? ModirPercent { get; set; }

    public int? Region { get; set; }

    public double? Farzand { get; set; }

    public double? Aeleh { get; set; }

    public int? Roshdsanavat { get; set; }

    public string? BimehAccount { get; set; }

    public string? BimehCAccount { get; set; }

    public int? Tahsilat1 { get; set; }

    public int? Tahsilat2 { get; set; }

    public int? Tahsilat3 { get; set; }

    public int? Tahsilat4 { get; set; }

    public double? Refahi { get; set; }

    public double? Maskan { get; set; }

    public double? Boan { get; set; }

    public virtual Region? RegionNavigation { get; set; }

    public virtual ICollection<SalaryPardakht> SalaryPardakhts { get; set; } = new List<SalaryPardakht>();

    public virtual ICollection<SalaryUserBase> SalaryUserBases { get; set; } = new List<SalaryUserBase>();
}
