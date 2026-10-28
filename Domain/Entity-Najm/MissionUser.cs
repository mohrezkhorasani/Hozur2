using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class MissionUser
{
    public int Id { get; set; }

    public int? Uid { get; set; }

    public int? Missid { get; set; }

    public virtual Mission? Miss { get; set; }
}
