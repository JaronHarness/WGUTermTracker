using SQLite;

namespace WGUTermTracker.Models;

public class Term : TrackableItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public override string GetSummary() =>
        $"Term: {Title} ({StartDate:yyyy-MM-dd} → {EndDate:yyyy-MM-dd})";
}
