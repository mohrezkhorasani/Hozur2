using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TUserAmoozesh
{
    public int Id { get; set; }

    public string? Fname { get; set; }

    public string? Lname { get; set; }

    public string? Faname { get; set; }

    public bool? Jensiat { get; set; }

    public string? Ss { get; set; }

    public string? Cm { get; set; }

    public string? Tt { get; set; }

    public string? MahalTavalod { get; set; }

    public string? MahalSodor { get; set; }

    public int? THozeh { get; set; }

    public string? TKelasic { get; set; }

    public string? Tabeeat { get; set; }

    public string? Reshteh { get; set; }

    public int? Tahol { get; set; }

    public string? Shoghl { get; set; }

    public string? Keshvar { get; set; }

    public string? Tel1 { get; set; }

    public string? Tel2 { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public DateOnly? DateSabet { get; set; }

    public bool? Enseraf { get; set; }

    public string? SMarkaz { get; set; }

    public string? SCode { get; set; }

    public bool? Hide { get; set; }

    public string? F1 { get; set; }

    public string? F2 { get; set; }

    public string? F3 { get; set; }

    public string? MajmaaOldCode { get; set; }

    public int? RegionId { get; set; }

    public bool? Internet { get; set; }

    public bool? Isview { get; set; }

    public Guid? Pcode { get; set; }

    public DateOnly? Registerdate { get; set; }

    public DateOnly? Lasteditdate { get; set; }

    public string? Tscode { get; set; }

    public string? Description { get; set; }

    public virtual Region? Region { get; set; }

    public virtual ICollection<SendSm> SendSms { get; set; } = new List<SendSm>();

    public virtual ICollection<TAmoozeshUserDoreh> TAmoozeshUserDorehs { get; set; } = new List<TAmoozeshUserDoreh>();

    public virtual ICollection<TAmoozeshUserHozor> TAmoozeshUserHozors { get; set; } = new List<TAmoozeshUserHozor>();
}
