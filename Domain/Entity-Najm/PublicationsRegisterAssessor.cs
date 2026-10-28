using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationsRegisterAssessor
{
    public int Id { get; set; }

    public Guid Gid { get; set; }

    public string Bookname { get; set; } = null!;

    public string Author { get; set; } = null!;

    public DateOnly Recivedate { get; set; }

    public int? Pages { get; set; }

    public int? Words { get; set; }

    public string? AssessorName { get; set; }

    public DateOnly? AssessorSenDate { get; set; }

    public DateOnly? AssessorReciveDate { get; set; }

    public int? AssessorResulte { get; set; }

    public string? Others { get; set; }

    public DateOnly? RegisterDate { get; set; }

    public virtual ICollection<PublicationsRegisterBook> PublicationsRegisterBooks { get; set; } = new List<PublicationsRegisterBook>();
}
