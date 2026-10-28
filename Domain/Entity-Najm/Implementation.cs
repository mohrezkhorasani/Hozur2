using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Implementation
{
    public int Id { get; set; }

    public string? Subject { get; set; }

    public string? Executive { get; set; }

    public int? Numofword { get; set; }

    public DateOnly? ImplementationInDate { get; set; }

    public DateOnly? ImplementationOutDate { get; set; }

    public DateOnly? TypeDate { get; set; }

    public DateOnly? OppositionInDate { get; set; }

    public DateOnly? OppositionOutDate { get; set; }

    public DateOnly? EditDate { get; set; }

    public DateOnly? PayDate { get; set; }

    public int? Numofmin { get; set; }

    public DateOnly? RegisterDate { get; set; }

    public int? Executiveid { get; set; }

    public int? Subjectid { get; set; }

    public virtual ImplementationUser? ExecutiveNavigation { get; set; }

    public virtual Implementationsubject? SubjectNavigation { get; set; }
}
