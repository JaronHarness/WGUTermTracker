using SQLite;
using WGUTermTracker.Models.Enums;

namespace WGUTermTracker.Models;

public class Course
{
    // Course Information
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Title { get; set; }
    public CourseStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Notes { get; set; }
    public bool EnableNotifications { get; set; }

    // Instructor Information
    public string InstructorName { get; set; }
    public string InstructorPhone { get; set; }
    public string InstructorEmail { get; set; }

    // Foreign Keys
    public int TermId { get; set; }

}
