using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Unit
{
    public int Id { get; set; }

    public string? Unitname { get; set; }

    public int? Mid { get; set; }

    public int? Parent { get; set; }

    public int? Leaveparent { get; set; }

    public string? Missionparent { get; set; }

    public virtual ICollection<Asset> AssetPlaceNavigations { get; set; } = new List<Asset>();

    public virtual ICollection<Asset> AssetUnitNavigations { get; set; } = new List<Asset>();

    public virtual ICollection<Unit> InverseParentNavigation { get; set; } = new List<Unit>();

    public virtual ICollection<Leave> Leaves { get; set; } = new List<Leave>();

    public virtual ICollection<Mission> Missions { get; set; } = new List<Mission>();

    public virtual Unit? ParentNavigation { get; set; }

    public virtual ICollection<UnitsUser> UnitsUsers { get; set; } = new List<UnitsUser>();
}
