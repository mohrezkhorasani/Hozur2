using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Medium
{
    public int Id { get; set; }

    public int? Mediatype { get; set; }

    public int? Subject { get; set; }

    public int? Subsubject { get; set; }

    public string? Medianame { get; set; }

    public int? Teacher { get; set; }

    public int? Statid { get; set; }

    public string? Locationfile { get; set; }

    public TimeOnly? Duration { get; set; }

    public int? Numbers { get; set; }

    public string? Description { get; set; }

    public Guid? Guid { get; set; }

    public DateOnly? Daterecive1 { get; set; }

    public DateOnly? Datedelivery1 { get; set; }

    public string? Applier1 { get; set; }

    public string? Amelejraee1 { get; set; }

    public DateOnly? Daterecive2 { get; set; }

    public DateOnly? Datedelivery2 { get; set; }

    public string? Applier2 { get; set; }

    public string? Amelejraee2 { get; set; }

    public string? Amelejraee3 { get; set; }

    public int? Mediaid { get; set; }

    public double? Farvast { get; set; }

    public string? Amelejraee4 { get; set; }

    public DateOnly? Dateproliferation { get; set; }

    public bool? Active1 { get; set; }

    public bool? Active2 { get; set; }

    public bool? Active3 { get; set; }

    public bool? Active4 { get; set; }

    public virtual PublicationCdregister? Media { get; set; }

    public virtual Users2? TeacherNavigation { get; set; }
}
