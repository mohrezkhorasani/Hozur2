using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TAmoozeshDorehStage
{
    public int Id { get; set; }

    public int DorehId { get; set; }

    public TimeOnly? TimeIn { get; set; }

    public TimeOnly? TimeOut { get; set; }

    public string? Descrip { get; set; }

    public int? TedadJaleseh { get; set; }

    public int? TedadHazer { get; set; }

    public DateOnly? Date { get; set; }

    public virtual TAmoozeshDoreh Doreh { get; set; } = null!;

    public virtual ICollection<TAmoozeshUserHozor> TAmoozeshUserHozors { get; set; } = new List<TAmoozeshUserHozor>();
}
