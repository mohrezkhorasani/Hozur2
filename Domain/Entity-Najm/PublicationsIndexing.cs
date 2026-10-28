using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationsIndexing
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public virtual PublicationsRegisterBook Book { get; set; } = null!;
}
