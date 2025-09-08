using SQLite;
using System.ComponentModel.DataAnnotations.Schema;
using WGUTermTracker.Models.Enums;

namespace WGUTermTracker.Models
{
    public class Assessment
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public AssessmentType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool EnableNotifications { get; set; }

        // Foreign Keys
        public int CourseId { get; set; }
    }
}
