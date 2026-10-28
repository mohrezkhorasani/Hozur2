using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class TeacherInfo
{
    public string? Fname { get; set; }

    public string? Lname { get; set; }

    public string? Faname { get; set; }

    public string? Bd { get; set; }

    public string? Ssn { get; set; }

    public string? Htel { get; set; }

    public string? Wtel { get; set; }

    public string? Mtel { get; set; }

    public string? Hadd { get; set; }

    public string? Wadd { get; set; }

    public string? CourseTeaching { get; set; }

    public string? CourseTeached { get; set; }

    public byte[]? Pic { get; set; }

    public string? Fd { get; set; }

    public int Id { get; set; }
}
