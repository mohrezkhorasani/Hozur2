using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class AccBase
{
    public int Id { get; set; }

    public int Year { get; set; }

    public double? Maskan { get; set; }

    public double? Boan { get; set; }

    public double? PayehAsTime { get; set; }

    public double? MinMaliat { get; set; }

    public int? MaliatPer { get; set; }

    public double? HaghOlad { get; set; }

    public int? KaranehPercent { get; set; }

    public int? EazafehPersent { get; set; }

    public string? ShobehBankName { get; set; }

    public string? ShobehCode { get; set; }

    public string? ShobehName { get; set; }

    public string? SazmanCode { get; set; }

    public string? SazmanName { get; set; }

    public string? SandooghName { get; set; }

    public string? SandooghHesab { get; set; }

    public string? SandooghNikname { get; set; }

    public string? BimehHesab { get; set; }

    public string? BimehTakmiliHesab { get; set; }

    public int? RegionId { get; set; }

    public bool? Lock { get; set; }

    public double? Taahol { get; set; }

    public virtual ICollection<AccBaseUser> AccBaseUsers { get; set; } = new List<AccBaseUser>();

    public virtual ICollection<AccPardakht> AccPardakhts { get; set; } = new List<AccPardakht>();

    public virtual Region? Region { get; set; }
}
