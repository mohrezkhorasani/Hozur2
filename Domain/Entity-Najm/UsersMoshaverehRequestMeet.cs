using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class UsersMoshaverehRequestMeet
{
    public int Id { get; set; }

    public string? Pid { get; set; }

    public int? Uid { get; set; }

    public int? Reqid { get; set; }

    public int? Ostadid { get; set; }

    public string? Tarikh { get; set; }

    public string? TimeIn { get; set; }

    public string? TimeOut { get; set; }

    public string? Modat { get; set; }

    public string? Nobat { get; set; }

    public string? Type { get; set; }

    public string? NextDate { get; set; }

    public string? NextTime { get; set; }

    public string? Kholaseh { get; set; }

    public string? Rahkar { get; set; }

    public string? Tozihat { get; set; }

    public int? ControlType { get; set; }

    public bool? Isdell { get; set; }

    public string? PayDate { get; set; }

    public double? PayAmount { get; set; }

    public string? TeachSendDate { get; set; }

    public string? TeachRecivDate { get; set; }

    public string? StudSendDate { get; set; }

    public string? StudRecivDate { get; set; }

    public bool? Isshow { get; set; }

    public int? Wordcount { get; set; }

    public DateOnly? TarikhEn { get; set; }

    public DateOnly? PaydateEn { get; set; }

    public virtual Users2? Ostad { get; set; }

    public virtual UsersMoshaverehRequest? Req { get; set; }

    public virtual UsersMoshavereh? UidNavigation { get; set; }
}
