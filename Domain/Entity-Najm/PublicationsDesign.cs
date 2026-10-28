using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationsDesign
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public DateOnly? DesignerDateSend { get; set; }

    public DateOnly? DesignerDateRecive { get; set; }

    public string? Designer { get; set; }

    public double? Cost { get; set; }

    public virtual PublicationsRegisterBook Book { get; set; } = null!;
}
