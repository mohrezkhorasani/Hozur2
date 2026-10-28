using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class UsersMoshavereh
{
    public int Id { get; set; }

    public string? Fname { get; set; }

    public string? Lname { get; set; }

    public string? Faname { get; set; }

    public string? Ss { get; set; }

    public string? Cm { get; set; }

    public string? MSdor { get; set; }

    public string? MTavalod { get; set; }

    public string? Tahol { get; set; }

    public string? Tt { get; set; }

    public string? Jensiat { get; set; }

    public string? THowzeh { get; set; }

    public string? TKelasic { get; set; }

    public string? Reshteh { get; set; }

    public string? MTahsil { get; set; }

    public string? Address { get; set; }

    public string? Tell1 { get; set; }

    public string? Tell2 { get; set; }

    public string? Tell3 { get; set; }

    public string? Email { get; set; }

    public string? Description { get; set; }

    public string? Uid { get; set; }

    public string? Pcode { get; set; }

    public int? ControlType { get; set; }

    public int? PanelId { get; set; }

    public DateOnly? Registerdate { get; set; }

    public DateOnly? Lasteditdate { get; set; }

    public bool? Isview { get; set; }

    public bool? Internet { get; set; }

    public int? RegionId { get; set; }

    public virtual Region? Region { get; set; }

    public virtual ICollection<UsersMoshaverehRequestMeet> UsersMoshaverehRequestMeets { get; set; } = new List<UsersMoshaverehRequestMeet>();

    public virtual ICollection<UsersMoshaverehRequest> UsersMoshaverehRequests { get; set; } = new List<UsersMoshaverehRequest>();
}
