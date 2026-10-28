using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class GroupsJalesehHzor
{
    public int Id { get; set; }

    public int? Jalesehid { get; set; }

    public int? UserId { get; set; }

    public string? Fname { get; set; }

    public string? Lname { get; set; }

    public DateOnly? Date { get; set; }

    public TimeOnly? TIn { get; set; }

    public TimeOnly? TOut { get; set; }

    public virtual GroupsJaleseh? Jaleseh { get; set; }
}
