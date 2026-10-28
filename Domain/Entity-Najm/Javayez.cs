using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Javayez
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Moshakhasat { get; set; }

    public virtual TUser User { get; set; } = null!;
}
