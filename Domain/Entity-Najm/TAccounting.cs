using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TAccounting
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? AccountNo { get; set; }

    public string? CardNo { get; set; }

    public string? BankAmel { get; set; }

    public int? SumMablagh { get; set; }

    public bool? Active { get; set; }

    public bool? DefaultCard { get; set; }
}
