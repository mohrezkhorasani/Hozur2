using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TAcc
{
    public int Id { get; set; }

    public int? Groupid { get; set; }

    public int? UserId { get; set; }

    public string? InTime { get; set; }

    public string? OutTime { get; set; }

    public DateTime? Date { get; set; }

    public bool? Hozor { get; set; }

    public int? Jid { get; set; }
}
