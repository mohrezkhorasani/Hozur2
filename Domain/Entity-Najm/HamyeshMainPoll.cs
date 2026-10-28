using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class HamyeshMainPoll
{
    public int Id { get; set; }

    public int HamId { get; set; }

    public string? TahsilatHowzeh { get; set; }

    public string? TahsialtKelasic { get; set; }

    public string? Sen { get; set; }

    public string? Name { get; set; }

    public string? Shoghl { get; set; }

    public string? EtelaResani { get; set; }

    public string? Pol1 { get; set; }

    public string? Pol2 { get; set; }

    public string? Pol3 { get; set; }

    public string? Pol4 { get; set; }

    public string? Pol5 { get; set; }

    public string? Pol6 { get; set; }

    public string? Pol7 { get; set; }

    public string? Pol8 { get; set; }

    public string? Pol9 { get; set; }

    public string? Pol10 { get; set; }

    public string? Pol11 { get; set; }

    public string? Pol12 { get; set; }

    public string? Pol13 { get; set; }

    public string? Pol14 { get; set; }

    public string? Pol15 { get; set; }

    public int? Trading { get; set; }

    public virtual HamyeshMain Ham { get; set; } = null!;
}
