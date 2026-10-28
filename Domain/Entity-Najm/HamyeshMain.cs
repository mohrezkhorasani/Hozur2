using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class HamyeshMain
{
    public int Id { get; set; }

    public string? Onvan { get; set; }

    public string? Makan { get; set; }

    public string? Date { get; set; }

    public string? Time { get; set; }

    public string? PollText1 { get; set; }

    public string? PollText2 { get; set; }

    public string? PollText3 { get; set; }

    public string? PollText4 { get; set; }

    public string? PollText5 { get; set; }

    public string? PollText6 { get; set; }

    public string? PollText7 { get; set; }

    public string? PollText8 { get; set; }

    public string? PollText9 { get; set; }

    public string? PollText10 { get; set; }

    public string? PollText11 { get; set; }

    public string? PollText12 { get; set; }

    public string? PollText13 { get; set; }

    public string? PollText14 { get; set; }

    public string? PollText15 { get; set; }

    public int? RegionId { get; set; }

    public virtual ICollection<HamyeshMainPoll> HamyeshMainPolls { get; set; } = new List<HamyeshMainPoll>();

    public virtual Region? Region { get; set; }
}
