using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TFile
{
    public int Id { get; set; }

    public string FileName { get; set; } = null!;

    public string? FileType { get; set; }

    public string? UserId { get; set; }

    public DateTime? Date { get; set; }

    public string? Fname { get; set; }

    public string? Lname { get; set; }

    public string? Code { get; set; }

    public byte[]? SelfFile { get; set; }

    public bool? Active { get; set; }

    public string? Descrpition { get; set; }

    public int? Tedadsafheh { get; set; }

    public int? Zaban { get; set; }

    public string? Other { get; set; }

    public DateTime? Date2 { get; set; }
}
