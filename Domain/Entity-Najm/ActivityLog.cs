using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class ActivityLog
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? Date { get; set; }

    public string? Time { get; set; }

    public string? ClassName { get; set; }

    public string? FunctionName { get; set; }

    public string? Action { get; set; }

    public string? Url { get; set; }

    public string? SessionId { get; set; }

    public string? Browser { get; set; }

    public string? Ip { get; set; }
}
