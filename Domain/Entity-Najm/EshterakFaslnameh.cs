using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class EshterakFaslnameh
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? Tel { get; set; }

    public bool? Active { get; set; }

    public string? Eshtaerakno { get; set; }

    public string? Codeposti { get; set; }

    public string? Fame { get; set; }

    public string? Lname { get; set; }

    public string? State { get; set; }

    public string? City { get; set; }

    public int? Credit { get; set; }

    public double? Amount { get; set; }
}
