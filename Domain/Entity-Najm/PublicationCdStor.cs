using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationCdStor
{
    public int Id { get; set; }

    public int? Cdid { get; set; }

    public int? CountCd { get; set; }

    public int? StoreType { get; set; }
}
