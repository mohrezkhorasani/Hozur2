using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationStore
{
    public int Id { get; set; }

    public int? Bookid { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public DateOnly? Logtostock { get; set; }

    public int? Circulation { get; set; }

    public int? Type { get; set; }

    public string? Description { get; set; }

    public double? Amount { get; set; }

    public int? Edition { get; set; }

    public virtual PublicationsRegisterBook? Book { get; set; }
}
