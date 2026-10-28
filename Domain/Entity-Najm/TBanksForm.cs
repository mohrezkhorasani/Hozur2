using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TBanksForm
{
    public int Id { get; set; }

    public int? Fid { get; set; }

    public string? Fyear { get; set; }

    public int? Fmonth { get; set; }

    public string? Datep { get; set; }

    public string? Kshobeh { get; set; }

    public string? Ksazman { get; set; }

    public string? Nameshobeh { get; set; }

    public int Kmarhaleh { get; set; }

    public string? Varasi { get; set; }

    public string? F1 { get; set; }

    public string? F2 { get; set; }

    public string? F3 { get; set; }
}
