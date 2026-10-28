using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TMoshaverehSabt
{
    public int Id { get; set; }

    public string? Userid { get; set; }

    public string? Requestid { get; set; }

    public int? Ostadid { get; set; }

    public DateTime? Jalesehdate { get; set; }

    public string? Saat { get; set; }

    public string? Ostadmail { get; set; }

    public string? Requestmail { get; set; }
}
