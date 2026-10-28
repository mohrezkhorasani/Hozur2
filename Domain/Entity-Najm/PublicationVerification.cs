using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationVerification
{
    public int Id { get; set; }

    public int? Bookid { get; set; }

    public DateOnly? AuthorDateSend { get; set; }

    public DateOnly? AuthorDateRecive { get; set; }

    public DateOnly? ReformsDate { get; set; }

    public DateOnly? SupervisorDateSend { get; set; }

    public DateOnly? SupervisorDateRecive { get; set; }

    public virtual PublicationsRegisterBook? Book { get; set; }
}
