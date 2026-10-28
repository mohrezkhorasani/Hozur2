using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class SalaryUserBase
{
    public int Id { get; set; }

    public int Uid { get; set; }

    public int? Bid { get; set; }

    public int? Taahol { get; set; }

    public int? Sabegheh { get; set; }

    public int? Masooliat { get; set; }

    public int? Masooliat2 { get; set; }

    public string? WorkType { get; set; }

    public int? Tahsilat { get; set; }

    public int? PercentModir { get; set; }

    public int? PercentEzafeh { get; set; }

    public int? ModirEjraee { get; set; }

    public double? Mpayeh { get; set; }

    public double? MpayehP { get; set; }

    public string? SHesab { get; set; }

    public string? Bank { get; set; }

    public int? Month { get; set; }

    public int? Year { get; set; }

    public bool? Locked { get; set; }

    public string? Decription { get; set; }

    public string? EditDate { get; set; }

    public int? Emtiaz { get; set; }

    public double? PayehEmtiaz { get; set; }

    public bool? Isgurd { get; set; }

    public int? Savabeghsanavat { get; set; }

    public double? ModirMablagh { get; set; }

    public int? FarzandTedad { get; set; }

    public int? Roshdsanavat { get; set; }

    public int? Tahsilat1 { get; set; }

    public int? Tahsilat2 { get; set; }

    public int? Tahsilat3 { get; set; }

    public int? Tahsilat4 { get; set; }

    public double? Refahi { get; set; }

    public int? IncomYear { get; set; }

    public int? ActiveYear { get; set; }

    public double? Maskan { get; set; }

    public double? Boan { get; set; }

    public virtual TAccBase? BidNavigation { get; set; }

    public virtual ICollection<SalaryUserList> SalaryUserLists { get; set; } = new List<SalaryUserList>();

    public virtual TUser UidNavigation { get; set; } = null!;
}
