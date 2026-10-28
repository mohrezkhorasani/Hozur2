using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class AccBaseUser
{
    public int Id { get; set; }

    public int Bid { get; set; }

    public int Uid { get; set; }

    public int? IncomYear { get; set; }

    public int? YearCount { get; set; }

    public double? Payeh { get; set; }

    public double? Maskan { get; set; }

    public double? Boan { get; set; }

    public int? OladCount { get; set; }

    public double? OladHagh { get; set; }

    public double? JamehKol { get; set; }

    public int? RegionId { get; set; }

    public int? Year { get; set; }

    public int? Month { get; set; }

    public string? ShomarehHesab { get; set; }

    public string? BankName { get; set; }

    public bool? Locked { get; set; }

    public bool? Taahol { get; set; }

    public double? Masouliat { get; set; }

    public double? TaaholHagh { get; set; }

    public virtual ICollection<AccBaseUserList> AccBaseUserLists { get; set; } = new List<AccBaseUserList>();

    public virtual AccBase BidNavigation { get; set; } = null!;

    public virtual TUser UidNavigation { get; set; } = null!;
}
