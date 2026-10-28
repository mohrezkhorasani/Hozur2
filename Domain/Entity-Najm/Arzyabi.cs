using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Arzyabi
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string? Arzyabiname { get; set; }

    public string? Maghalehname { get; set; }

    public string? Nevisandeh { get; set; }

    public string? Dateersal { get; set; }

    public string? Datedaryaft { get; set; }

    public bool? Mortabet { get; set; }

    public string? Nazarat { get; set; }

    public string? Tozihat { get; set; }

    public string? CodeMaghaleh { get; set; }

    public string? CodeNevisandeh { get; set; }

    public string? Melak { get; set; }

    public string? Emtiaz { get; set; }

    public string? Natijeh { get; set; }

    public string? TozihVanazar { get; set; }

    public string? EslahatSori { get; set; }

    public string? EslahatMohtavaee { get; set; }

    public string? Chap { get; set; }

    public string? Dalil { get; set; }
}
