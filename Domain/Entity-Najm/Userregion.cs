using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Userregion
{
    public int Id { get; set; }

    public int RegionId { get; set; }

    public int UserId { get; set; }

    public virtual Region Region { get; set; } = null!;

    public virtual TUser User { get; set; } = null!;
}
