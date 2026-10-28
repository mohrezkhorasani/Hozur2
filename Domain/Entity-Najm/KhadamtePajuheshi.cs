using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class KhadamtePajuheshi
{
    public int Id { get; set; }

    public string? Type { get; set; }

    public string? Title { get; set; }

    public string? Subject { get; set; }

    public string? Writer { get; set; }

    public string? PresentationDate { get; set; }

    public string? Penman { get; set; }

    public string? Refers { get; set; }

    public string? Typist { get; set; }

    public int? HwPageCount { get; set; }

    public int? PrimaryWordCount { get; set; }

    public int? FinalPageCount { get; set; }

    public int? FinalWordCount { get; set; }

    public string? Transferee { get; set; }

    public string? DateOfComp { get; set; }

    public string? RegisterationNum { get; set; }

    public virtual ICollection<KhadamtePajuheshi2> KhadamtePajuheshi2s { get; set; } = new List<KhadamtePajuheshi2>();
}
