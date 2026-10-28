using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationsRepository
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public int? Type { get; set; }

    public bool? First { get; set; }

    public DateOnly? InputDate { get; set; }

    public int? Number { get; set; }

    public virtual PublicationsRegisterBook Book { get; set; } = null!;
}
