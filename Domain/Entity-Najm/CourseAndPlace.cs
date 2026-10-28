using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class CourseAndPlace
{
    public string? CourseName { get; set; }

    public string? Place { get; set; }

    public string? Hours { get; set; }

    public string? Year { get; set; }

    public int Id { get; set; }
}
