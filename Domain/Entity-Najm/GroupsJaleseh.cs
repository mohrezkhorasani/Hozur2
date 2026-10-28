using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class GroupsJaleseh
{
    public int Id { get; set; }

    public string? GroupId { get; set; }

    public int? UserId { get; set; }

    public int? EraeUser { get; set; }

    public string? EraeUserName { get; set; }

    public string? Note { get; set; }

    public DateTime? Date { get; set; }

    public string? Tag { get; set; }

    public string? Onvan { get; set; }

    public string? Mahal { get; set; }

    public int? ManagerId { get; set; }

    public int DabirId { get; set; }

    public string? STime { get; set; }

    public string? ETime { get; set; }

    public string? Dastor { get; set; }

    public string? Mozakerat { get; set; }

    public string? NDastor { get; set; }

    public int? Ntype { get; set; }

    public int? Sindex { get; set; }

    public string? Managername { get; set; }

    public string? Dabirname { get; set; }

    public virtual Group? Group { get; set; }

    public virtual ICollection<GroupsJalesehHzor> GroupsJalesehHzors { get; set; } = new List<GroupsJalesehHzor>();
}
