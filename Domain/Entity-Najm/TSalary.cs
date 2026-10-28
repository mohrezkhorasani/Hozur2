using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TSalary
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? Emtiaz { get; set; }

    public double? Bmablagh { get; set; }

    public double? Saat { get; set; }

    public double? EzafehKari { get; set; }

    public double? Smamoriat { get; set; }

    public double? Spadash { get; set; }

    public double? Sbimeh { get; set; }

    public double? MablaghNahaee { get; set; }

    public double? Khales { get; set; }

    public double? Maliat { get; set; }

    public bool? KasrBimeh { get; set; }

    public int? Month { get; set; }

    public int? Year { get; set; }

    public string ShomarehHesab { get; set; } = null!;

    public string? Datefrom { get; set; }

    public string? DateTo { get; set; }

    public int? Bank { get; set; }

    public int? MaliatPercent { get; set; }

    public int? Taahol { get; set; }

    public int? Tahsilat { get; set; }

    public int? Sabegheh { get; set; }

    public int? Masooliat { get; set; }

    public string? Pdate { get; set; }

    public string? Other { get; set; }

    public double? Bestankari { get; set; }

    public string? Describ { get; set; }

    public int? Masooliatmozaf { get; set; }
}
