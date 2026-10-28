using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationIndexing
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public string? Indextype { get; set; }

    public bool? Method { get; set; }

    public double? AmountPerPage { get; set; }

    public double? AmountTotal { get; set; }

    public DateOnly? SendDate { get; set; }

    public string? IndexerName { get; set; }

    public DateOnly? ReciveDate { get; set; }

    public virtual PublicationsRegisterBook Book { get; set; } = null!;
}
