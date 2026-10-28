using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class SendSm
{
    public int Id { get; set; }

    public Guid? Guid { get; set; }

    public int? Code { get; set; }

    public DateTime? Senddate { get; set; }

    public int? Userid { get; set; }

    public virtual TUserAmoozesh? User { get; set; }
}
