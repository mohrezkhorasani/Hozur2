using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationsPrinting
{
    public int Id { get; set; }

    public int? BookId { get; set; }

    public DateOnly? PrinterySendDate { get; set; }

    public string? PrinteryName { get; set; }

    public double? AmountPrinteryTotal { get; set; }

    public int? FactorId { get; set; }

    public DateOnly? PrintingEndDate { get; set; }

    public bool? ConfirmationOfReceipt { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public virtual PublicationsRegisterBook? Book { get; set; }
}
