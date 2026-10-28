using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Taddress
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? Faddress { get; set; }

    public int? Ftype { get; set; }

    public string? Tel1 { get; set; }

    public string? Tel2 { get; set; }

    public string? Mobile1 { get; set; }

    public string? Mobile2 { get; set; }

    public string? Fax1 { get; set; }

    public string? Fax2 { get; set; }

    public string? CodePosti { get; set; }

    public string? Email1 { get; set; }

    public string? Email2 { get; set; }

    public string? Website1 { get; set; }

    public string? Website2 { get; set; }

    public virtual TUser? User { get; set; }
}
