using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Dost
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? Dname { get; set; }

    public string? Semat { get; set; }

    public string? MahalKar { get; set; }

    public string? Daddress { get; set; }

    public virtual TUser? User { get; set; }
}
