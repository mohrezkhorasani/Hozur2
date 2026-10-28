using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class Zaban
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? ZabanName { get; set; }

    public int? DarkMatlab { get; set; }

    public int? Tarjomeh { get; set; }

    public int? Mokalemeh { get; set; }

    public string? Describ { get; set; }

    public virtual TUser User { get; set; } = null!;
}
