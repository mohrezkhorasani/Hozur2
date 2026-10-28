using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class CongressMainUserArticle
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Code { get; set; }

    public int? Congid { get; set; }

    public int? Uid { get; set; }

    public DateOnly? AdedDate { get; set; }

    public bool? Active { get; set; }

    public int? Tedadsafheh { get; set; }

    public int? Type { get; set; }

    public int? Language { get; set; }

    public DateOnly? ReciveDate { get; set; }

    public DateOnly? FullReciveDate { get; set; }

    public int? Nofword { get; set; }

    public virtual CongressMain? Cong { get; set; }

    public virtual ICollection<CongressMainUserArticlesRating> CongressMainUserArticlesRatings { get; set; } = new List<CongressMainUserArticlesRating>();

    public virtual CongressMainUser? UidNavigation { get; set; }
}
