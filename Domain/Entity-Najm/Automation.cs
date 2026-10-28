using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Automation
{
    public int Id { get; set; }

    public Guid Gid { get; set; }

    public int Sender { get; set; }

    public int Reciver { get; set; }

    public int Rsecivertype { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }

    public int? TopMessage { get; set; }

    public bool Attach { get; set; }

    public bool Islock { get; set; }

    public DateOnly Datesend { get; set; }

    public DateOnly? Dateseen { get; set; }

    public int Priority { get; set; }

    public bool Draft { get; set; }

    public virtual ICollection<Automation> InverseTopMessageNavigation { get; set; } = new List<Automation>();

    public virtual LoginUser ReciverNavigation { get; set; } = null!;

    public virtual LoginUser SenderNavigation { get; set; } = null!;

    public virtual Automation? TopMessageNavigation { get; set; }
}
