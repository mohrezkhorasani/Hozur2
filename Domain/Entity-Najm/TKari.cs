using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TKari
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? Type { get; set; }

    public string? Mozoo { get; set; }

    public string? Mahal { get; set; }

    public string? Semat { get; set; }

    public int? Modat { get; set; }

    public int? NFaaliat { get; set; }

    public string? DateIn { get; set; }

    public string? DateOut { get; set; }

    public virtual TUser? User { get; set; }
}
