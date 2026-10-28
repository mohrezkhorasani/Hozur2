using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class UserRole
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public int? UserId { get; set; }

    public virtual Role Role { get; set; } = null!;
}
