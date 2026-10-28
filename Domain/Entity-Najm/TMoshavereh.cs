using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TMoshavereh
{
    public int Id { get; set; }

    public string? MoshaverId { get; set; }

    public string? MoteghaziId { get; set; }

    public string? Requestid { get; set; }

    public DateTime? Jdat { get; set; }

    public string? Zin { get; set; }

    public string? Zout { get; set; }

    public TimeOnly? Modat { get; set; }

    public string? Nobat { get; set; }

    public bool? Next { get; set; }

    public DateTime? Ndate { get; set; }

    public string? Ntime { get; set; }

    public string? Kholaseh { get; set; }

    public string? Rahkar { get; set; }

    public string? Tozihat { get; set; }

    public string? Mtype { get; set; }
}
