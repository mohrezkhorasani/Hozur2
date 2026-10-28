using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TTalif
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Onvan { get; set; }

    public int? TarOrTal { get; set; }

    public int? Ktype { get; set; }

    public string? Mozo { get; set; }

    public string? Nasher { get; set; }

    public int? Sal { get; set; }

    public int? Zaban { get; set; }

    public virtual TUser User { get; set; } = null!;
}
