using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class SalaryUserPayBase
{
    public int Id { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public double? OstadJ { get; set; }

    public double? OstadS { get; set; }

    public double? GroupJ { get; set; }

    public double? GroupS { get; set; }

    public double? MoshaverehJ { get; set; }

    public double? MoshaverehS { get; set; }

    public double? HeyatJ { get; set; }

    public double? HeyatS { get; set; }

    public double? MaliatPercent { get; set; }

    public string? Onvan { get; set; }

    public virtual ICollection<SalaryUserPayGroup> SalaryUserPayGroups { get; set; } = new List<SalaryUserPayGroup>();
}
