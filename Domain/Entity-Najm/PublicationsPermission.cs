using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationsPermission
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public DateOnly? Shabakdate { get; set; }

    public string? Shabakno { get; set; }

    public DateOnly? FipaSendDate { get; set; }

    public DateOnly? FipaReciveDate { get; set; }

    public string? NumberofNl { get; set; }

    public DateOnly? ErshadSendDate { get; set; }

    public DateOnly? ErshadReciveDate { get; set; }

    public string? Description { get; set; }

    public virtual PublicationsRegisterBook Book { get; set; } = null!;
}
