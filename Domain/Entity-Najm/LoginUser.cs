using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class LoginUser
{
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int UserId { get; set; }

    public int Id { get; set; }

    public virtual ICollection<Automation> AutomationReciverNavigations { get; set; } = new List<Automation>();

    public virtual ICollection<Automation> AutomationSenderNavigations { get; set; } = new List<Automation>();

    public virtual TUser User { get; set; } = null!;
}
