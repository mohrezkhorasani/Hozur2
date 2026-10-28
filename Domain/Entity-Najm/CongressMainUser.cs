using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class CongressMainUser
{
    public int Id { get; set; }

    public int? Congid { get; set; }

    public string? Fname { get; set; }

    public string? Lname { get; set; }

    public string? Code { get; set; }

    public string? Codemelli { get; set; }

    public string? Decription { get; set; }

    public int? Type { get; set; }

    public DateOnly? AddedDate { get; set; }

    public string? Email { get; set; }

    public string? Tel { get; set; }

    public virtual CongressMain? Cong { get; set; }

    public virtual ICollection<CongressMainUserArticle> CongressMainUserArticles { get; set; } = new List<CongressMainUserArticle>();

    public virtual ICollection<CongressMainUserArticlesRating> CongressMainUserArticlesRatingUidAuthorNavigations { get; set; } = new List<CongressMainUserArticlesRating>();

    public virtual ICollection<CongressMainUserArticlesRating> CongressMainUserArticlesRatingUidPreferentialNavigations { get; set; } = new List<CongressMainUserArticlesRating>();
}
