using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TPayanCounter
{
    public int Id { get; set; }

    public int? Counter { get; set; }

    public DateOnly? Date { get; set; }
}
