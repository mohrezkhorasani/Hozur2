using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Leave
{
    public int Id { get; set; }

    public int? Uid { get; set; }

    public int? Unitid { get; set; }

    public DateOnly? Date { get; set; }

    public DateOnly? Datestart { get; set; }

    public DateOnly? Dateend { get; set; }

    public TimeOnly? Timestart { get; set; }

    public TimeOnly? Timeend { get; set; }

    public int? Type { get; set; }

    public bool? Agreement { get; set; }

    public bool? Active { get; set; }

    public int? Parentuid2 { get; set; }

    public int? Parentuid1 { get; set; }

    public virtual Unit? Unit { get; set; }
}
