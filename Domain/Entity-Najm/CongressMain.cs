using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class CongressMain
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Makan { get; set; }

    public DateOnly? AdedDate { get; set; }

    public DateOnly? DateFrom { get; set; }

    public DateOnly? DateTo { get; set; }

    public string? OtherSponser { get; set; }

    public bool? OnlyMjma { get; set; }

    public int? Charachterinpage { get; set; }

    public virtual ICollection<CongressMainUserArticle> CongressMainUserArticles { get; set; } = new List<CongressMainUserArticle>();

    public virtual ICollection<CongressMainUserArticlesRating> CongressMainUserArticlesRatings { get; set; } = new List<CongressMainUserArticlesRating>();

    public virtual ICollection<CongressMainUser> CongressMainUsers { get; set; } = new List<CongressMainUser>();
}
