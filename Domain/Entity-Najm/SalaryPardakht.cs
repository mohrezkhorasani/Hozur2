using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class SalaryPardakht
{
    public int Id { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public string Pdate { get; set; } = null!;

    public string? Vdate { get; set; }

    public int Tedad { get; set; }

    public string? FileMatn { get; set; }

    public string? CodeMarhaleh { get; set; }

    public double? Mablagh { get; set; }

    public string? MablaghHrof { get; set; }

    public string? Varesi { get; set; }

    public string? CodeShobeh { get; set; }

    public string? CodeSazman { get; set; }

    public string? NameShobeh { get; set; }

    public string? Filename { get; set; }

    public string? DateSabt { get; set; }

    public string? DescAll { get; set; }

    public bool? Islock { get; set; }

    public double? FundTotal { get; set; }

    public int? FundMembersNumber { get; set; }

    public string? FundAccount { get; set; }

    public string? FundBank { get; set; }

    public string? FundName { get; set; }

    public string? FundAlias { get; set; }

    public double? Maliat { get; set; }

    public double? Bimeh { get; set; }

    public double? Khales { get; set; }

    public int? Accb { get; set; }

    public double? BimehC { get; set; }

    public double? BimehHekmat { get; set; }

    public double? BimehCHekmat { get; set; }

    public virtual TAccBase? AccbNavigation { get; set; }

    public virtual ICollection<SalaryUserList> SalaryUserLists { get; set; } = new List<SalaryUserList>();
}
