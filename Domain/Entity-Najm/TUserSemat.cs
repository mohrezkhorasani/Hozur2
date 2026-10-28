using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TUserSemat
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SematId { get; set; }

    public bool DefSemat { get; set; }
}
