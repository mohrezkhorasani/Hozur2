using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TAmoozeshUserDoreh
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int DorehId { get; set; }

    public DateTime? Date { get; set; }

    public bool? Enseraf { get; set; }

    public string? Tscode { get; set; }

    public string? Description { get; set; }

    public virtual TAmoozeshDoreh Doreh { get; set; } = null!;

    public virtual TUserAmoozesh User { get; set; } = null!;
}
