using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class KhadamtePajuheshi2
{
    public int Id { get; set; }

    public string? Eslahat { get; set; }

    public string? Erja { get; set; }

    public string? Mojri { get; set; }

    public int? KalamatNahaei { get; set; }

    public int? SafahatNahaei { get; set; }

    public string? TarikheEslah { get; set; }

    public int? TId { get; set; }

    public virtual KhadamtePajuheshi? TIdNavigation { get; set; }
}
