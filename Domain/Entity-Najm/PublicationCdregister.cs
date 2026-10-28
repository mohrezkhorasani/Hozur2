using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationCdregister
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Teachername { get; set; }

    public int? Subject { get; set; }

    public int? Classtype { get; set; }

    public double? FarvastNo { get; set; }

    public double? Price { get; set; }

    public virtual ICollection<Medium> Media { get; set; } = new List<Medium>();
}
