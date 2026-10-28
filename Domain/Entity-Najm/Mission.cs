using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Mission
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

    public string? Disagreennote { get; set; }

    public string? Missionplace { get; set; }

    public string? MissionReason { get; set; }

    public int? Vehicle { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<MissionUser> MissionUsers { get; set; } = new List<MissionUser>();

    public virtual Unit? Unit { get; set; }
}
