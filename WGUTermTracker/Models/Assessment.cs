using SQLite;
using WGUTermTracker.Models.Enums;

namespace WGUTermTracker.Models
{
    public class Assessment : TrackableItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public AssessmentType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool EnableNotifications { get; set; }
        public int CourseId { get; set; }

        public override string GetSummary() =>
            $"Assessment: {Title} ({Type}) {StartDate:yyyy-MM-dd} → {EndDate:yyyy-MM-dd}";
    }
}
