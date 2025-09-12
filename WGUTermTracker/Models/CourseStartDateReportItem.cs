using WGUTermTracker.Models.Enums;

namespace WGUTermTracker.Models
{
    public class CourseStartDateReportItem
    {
        public string Title { get; set; }
        public CourseStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TermTitle { get; set; }
    }
}
