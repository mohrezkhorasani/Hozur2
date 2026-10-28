using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class UsersMoshaverehRequest
{
    public int Id { get; set; }

    public int? Uid { get; set; }

    public string? DateSabt { get; set; }

    public string? Zamineh { get; set; }

    public string? Maghta { get; set; }

    public string? Mozoo { get; set; }

    public string? ORahnama { get; set; }

    public string? OMoshaver { get; set; }

    public string? Mozo { get; set; }

    public string? SAsli { get; set; }

    public string? Description { get; set; }

    public string? Saat1 { get; set; }

    public string? Saat2 { get; set; }

    public string? Saat3 { get; set; }

    public string? Saat4 { get; set; }

    public string? Saat5 { get; set; }

    public string? Saat6 { get; set; }

    public bool? IsDell { get; set; }

    public string? PcodeRequest { get; set; }

    public int? ControlType { get; set; }

    public bool? IsFinall { get; set; }

    public int? TopId { get; set; }

    public string? DateRequest { get; set; }

    public DateTime? RegDate { get; set; }

    public bool? Isview { get; set; }

    public bool? Internet { get; set; }

    public virtual UsersMoshavereh? UidNavigation { get; set; }

    public virtual ICollection<UsersMoshaverehRequestMeet> UsersMoshaverehRequestMeets { get; set; } = new List<UsersMoshaverehRequestMeet>();
}
