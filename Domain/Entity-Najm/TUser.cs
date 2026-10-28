using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TUser
{
    public int Id { get; set; }

    public string Fname { get; set; } = null!;

    public string Lname { get; set; } = null!;

    public string? Faname { get; set; }

    public string? SShenasnameh { get; set; }

    public string? KodeMelli { get; set; }

    public string? MahalSodorshenasname { get; set; }

    public string? MahalTavalod { get; set; }

    public string? Tarikhtavalod { get; set; }

    public string? Tabeiat { get; set; }

    public string? RotbehElmi { get; set; }

    public int? TVorodHozeh { get; set; }

    public string? SOzviat { get; set; }

    public DateTime? TOzviat { get; set; }

    public string NOzviat { get; set; } = null!;

    public string? NHamkari { get; set; }

    public int? AkMadrak { get; set; }

    public bool? Vaziat { get; set; }

    public byte[]? Tasvir { get; set; }

    public string? SParvandehMarkazModiriat { get; set; }

    public bool? Taahol { get; set; }

    public bool? Jensiat { get; set; }

    public int? Shoghl { get; set; }

    public string? SOzviatketabkhaneh { get; set; }

    public bool? TaeedOzviatKetabkhaneh { get; set; }

    public string? Descripton { get; set; }

    public string? Username { get; set; }

    public string? Userpass { get; set; }

    public string? UserId { get; set; }

    public int? Masoliat { get; set; }

    public int? TahsilatHozew { get; set; }

    public DateTime? DateStart { get; set; }

    public DateTime? DateEnd { get; set; }

    public string? OnvanPayan3 { get; set; }

    public string? OnvanPayan4 { get; set; }

    public int? Sabegheh { get; set; }

    public bool? F1 { get; set; }

    public bool? F2 { get; set; }

    public int? Masooliatmozaf { get; set; }

    public string? SHesab { get; set; }

    public string? Bank { get; set; }

    public bool? HamkariVaziat { get; set; }

    public virtual ICollection<AccBaseUserList> AccBaseUserLists { get; set; } = new List<AccBaseUserList>();

    public virtual ICollection<AccBaseUser> AccBaseUsers { get; set; } = new List<AccBaseUser>();

    public virtual ICollection<Dost> Dosts { get; set; } = new List<Dost>();

    public virtual ICollection<Group> GroupDabirs { get; set; } = new List<Group>();

    public virtual ICollection<Group> GroupPajoheshes { get; set; } = new List<Group>();

    public virtual ICollection<Group> GroupUsers { get; set; } = new List<Group>();

    public virtual ICollection<Javayez> Javayezs { get; set; } = new List<Javayez>();

    public virtual ICollection<LoginUser> LoginUsers { get; set; } = new List<LoginUser>();

    public virtual ICollection<SalaryUserBase> SalaryUserBases { get; set; } = new List<SalaryUserBase>();

    public virtual ICollection<SalaryUserList> SalaryUserLists { get; set; } = new List<SalaryUserList>();

    public virtual ICollection<TKari> TKaris { get; set; } = new List<TKari>();

    public virtual ICollection<TTalif> TTalifs { get; set; } = new List<TTalif>();

    public virtual ICollection<Taddress> Taddresses { get; set; } = new List<Taddress>();

    public virtual ICollection<Userregion> Userregions { get; set; } = new List<Userregion>();

    public virtual ICollection<Zaban> Zabans { get; set; } = new List<Zaban>();
}
