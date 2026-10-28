using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationsBookEdit
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public DateOnly? SendDate { get; set; }

    public string? EditorName { get; set; }

    public int? EditType { get; set; }

    public double? EditPerPage { get; set; }

    public double? EditTotal { get; set; }

    public DateOnly? EditorReciveDate { get; set; }

    public DateOnly? EditorStartDate { get; set; }

    public DateOnly? EditorEndDate { get; set; }

    public DateOnly? CheckingSendDate { get; set; }

    public string? CheckingName { get; set; }

    public double? CheckingPerPage { get; set; }

    public double? CheckingTotal { get; set; }

    public DateOnly? CheckingReciveDate { get; set; }

    public DateOnly? ReformsStartDate { get; set; }

    public DateOnly? ReformsEndDate { get; set; }

    public DateOnly? BookAuthorSendDate { get; set; }

    public DateOnly? BookAuthorReciveDate { get; set; }

    public DateOnly? BookAuthorReforms { get; set; }

    public DateOnly? AuthorLetter { get; set; }

    public virtual PublicationsRegisterBook Book { get; set; } = null!;
}
