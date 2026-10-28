using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TAmoozeshDoreh
{
    public int Id { get; set; }

    public int Shomareh { get; set; }

    public string Onvan { get; set; } = null!;

    public int Tedadjaleseh { get; set; }

    public int OstadId { get; set; }

    public DateOnly? DateStart { get; set; }

    public DateOnly? DateEnd { get; set; }

    public string? Ayam { get; set; }

    public string? Saat { get; set; }

    public string? Sharayet { get; set; }

    public string? Descrbtion { get; set; }

    public int? Type { get; set; }

    public int? Jensiat { get; set; }

    public string? Sal { get; set; }

    public string? Makan { get; set; }

    public bool? Active { get; set; }

    public string? F1 { get; set; }

    public string? F2 { get; set; }

    public string? F3 { get; set; }

    public int RegionId { get; set; }

    public int? TeacherId { get; set; }

    public virtual Region Region { get; set; } = null!;

    public virtual ICollection<TAmoozeshDorehStage> TAmoozeshDorehStages { get; set; } = new List<TAmoozeshDorehStage>();

    public virtual ICollection<TAmoozeshUserDoreh> TAmoozeshUserDorehs { get; set; } = new List<TAmoozeshUserDoreh>();

    public virtual Users2? Teacher { get; set; }
}
