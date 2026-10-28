using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Users2
{
    public int Id { get; set; }

    public string? Uid { get; set; }

    public string? Fname { get; set; }

    public string? Lname { get; set; }

    public string? Faname { get; set; }

    public string? Ss { get; set; }

    public string? Tt { get; set; }

    public string? Cm { get; set; }

    public string? MTavalod { get; set; }

    public string? Shahr { get; set; }

    public string? THowzeh { get; set; }

    public string? TKelasic { get; set; }

    public string? Reshteh { get; set; }

    public string? LMadrak { get; set; }

    public string? MTahsil { get; set; }

    public string? Takhasos { get; set; }

    public string? KarType { get; set; }

    public string? MahalKar { get; set; }

    public string? AKar { get; set; }

    public string? AManzel { get; set; }

    public string? Masooliat { get; set; }

    public string? TelKar { get; set; }

    public string? TelManzel { get; set; }

    public string? TelMobile { get; set; }

    public string? Email { get; set; }

    public string? Hesab { get; set; }

    public string? Bank { get; set; }

    public string? BankShobeh { get; set; }

    public string? BankType { get; set; }

    public string? HamkariType { get; set; }

    public string? HamkariNahweh { get; set; }

    public int? ControlType { get; set; }

    public string? Laghab { get; set; }

    public int? Region { get; set; }

    public virtual ICollection<Medium> Media { get; set; } = new List<Medium>();

    public virtual Region? RegionNavigation { get; set; }

    public virtual ICollection<TAmoozeshDoreh> TAmoozeshDorehs { get; set; } = new List<TAmoozeshDoreh>();

    public virtual ICollection<UsersMoshaverehRequestMeetPoll> UsersMoshaverehRequestMeetPolls { get; set; } = new List<UsersMoshaverehRequestMeetPoll>();

    public virtual ICollection<UsersMoshaverehRequestMeet> UsersMoshaverehRequestMeets { get; set; } = new List<UsersMoshaverehRequestMeet>();
}
