using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Group
{
    public int Id { get; set; }

    public string GroupId { get; set; } = null!;

    public int? UserId { get; set; }

    public int DabirId { get; set; }

    public string? GroupName { get; set; }

    public string? AsasNameh { get; set; }

    public DateTime? BuildDate { get; set; }

    public string? Description { get; set; }

    public DateTime? LastJaleseh { get; set; }

    public string? RoozJalaseh { get; set; }

    public int? Dayormonth { get; set; }

    public int? RegionId { get; set; }

    public int? Pajoheshid { get; set; }

    public virtual TUser Dabir { get; set; } = null!;

    public virtual ICollection<GroupsJaleseh> GroupsJalesehs { get; set; } = new List<GroupsJaleseh>();

    public virtual TUser? Pajohesh { get; set; }

    public virtual Region? Region { get; set; }

    public virtual TUser? User { get; set; }
}
