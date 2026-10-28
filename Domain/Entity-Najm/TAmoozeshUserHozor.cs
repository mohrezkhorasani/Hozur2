using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TAmoozeshUserHozor
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Descrip { get; set; }

    public TimeOnly? TimeIn { get; set; }

    public TimeOnly? TimeOut { get; set; }

    public int Stage { get; set; }

    public virtual TAmoozeshDorehStage StageNavigation { get; set; } = null!;

    public virtual TUserAmoozesh User { get; set; } = null!;
}
