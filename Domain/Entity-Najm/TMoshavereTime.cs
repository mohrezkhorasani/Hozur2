using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TMoshavereTime
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public string? DateSave { get; set; }

    public int? Zamineh { get; set; }

    public string? MozooFaree { get; set; }

    public string? OstdaRahnama { get; set; }

    public string? Ostadmoshaver { get; set; }

    public string? MozooMoshavereh { get; set; }

    public string? ZamanPishnahadi { get; set; }

    public string? Soalasli { get; set; }

    public string? Decrip { get; set; }

    public string? Code2 { get; set; }

    public bool? Final { get; set; }

    public int? Maghta { get; set; }
}
