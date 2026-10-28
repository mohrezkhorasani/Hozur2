using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Region
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AccBase> AccBases { get; set; } = new List<AccBase>();

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

    public virtual ICollection<FundUsersBase> FundUsersBases { get; set; } = new List<FundUsersBase>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<HamyeshMain> HamyeshMains { get; set; } = new List<HamyeshMain>();

    public virtual ICollection<TAccBase> TAccBases { get; set; } = new List<TAccBase>();

    public virtual ICollection<TAccFactor> TAccFactors { get; set; } = new List<TAccFactor>();

    public virtual ICollection<TAmoozeshDoreh> TAmoozeshDorehs { get; set; } = new List<TAmoozeshDoreh>();

    public virtual ICollection<TUserAmoozesh> TUserAmoozeshes { get; set; } = new List<TUserAmoozesh>();

    public virtual ICollection<Userregion> Userregions { get; set; } = new List<Userregion>();

    public virtual ICollection<Users2> Users2s { get; set; } = new List<Users2>();

    public virtual ICollection<UsersMoshavereh> UsersMoshaverehs { get; set; } = new List<UsersMoshavereh>();
}
