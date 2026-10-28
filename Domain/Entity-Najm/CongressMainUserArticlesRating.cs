using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class CongressMainUserArticlesRating
{
    public int Id { get; set; }

    public int? Congid { get; set; }

    public int? UidPreferential { get; set; }

    public int? UidAuthor { get; set; }

    public DateOnly? DateSend { get; set; }

    public DateOnly? DateRecive { get; set; }

    public bool? Mortabet { get; set; }

    public string? Nazar { get; set; }

    public string? Decription { get; set; }

    public int? Score { get; set; }

    public bool? AllowPrint { get; set; }

    public bool? AllowShow { get; set; }

    public int? ArticleId { get; set; }

    public bool? AllowSpeech { get; set; }

    public bool? Accept { get; set; }

    public bool? Reform { get; set; }

    public bool? AllowAbstract { get; set; }

    public virtual CongressMainUserArticle? Article { get; set; }

    public virtual CongressMain? Cong { get; set; }

    public virtual CongressMainUser? UidAuthorNavigation { get; set; }

    public virtual CongressMainUser? UidPreferentialNavigation { get; set; }
}
