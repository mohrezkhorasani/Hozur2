using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class SalaryUserPay
{
    public int Id { get; set; }

    public int? SanadSh { get; set; }

    public int? Uid { get; set; }

    public int Bid { get; set; }

    public string? Onvan { get; set; }

    public string? Name { get; set; }

    public string? SHesab { get; set; }

    public string? Bank { get; set; }

    public string? Babat { get; set; }

    public string? OzviatType { get; set; }

    public string? GroupElmi { get; set; }

    public string? DateFrom { get; set; }

    public string? Dateto { get; set; }

    public int? Tedad { get; set; }

    public int? Saat { get; set; }

    public double? MVahedJ { get; set; }

    public double? MVahedS { get; set; }

    public double? MKol { get; set; }

    public double? MaliatPercent { get; set; }

    public string? MMaliat { get; set; }

    public double? MKhales { get; set; }

    public string? DateP { get; set; }

    public string? Descrption { get; set; }

    public int? Year { get; set; }

    public int? Month { get; set; }
}
