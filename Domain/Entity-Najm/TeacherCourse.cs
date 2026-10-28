using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TeacherCourse
{
    public int TId { get; set; }

    public string? TName { get; set; }

    public int PId { get; set; }

    public string? PCourseName { get; set; }

    public string? PPlace { get; set; }

    public string? PHours { get; set; }

    public byte[]? TPic { get; set; }

    public int Id { get; set; }
}
