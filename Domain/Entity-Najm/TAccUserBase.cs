using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TAccUserBase
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BaseMablagh { get; set; }

    public int Fyear { get; set; }

    public int? Fmonth { get; set; }

    public int? ModirPercent { get; set; }

    public int? NewBasemablagh { get; set; }
}
