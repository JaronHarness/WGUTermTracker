using WGUTermTracker.Models;
using WGUTermTracker.Models.Enums;
using WGUTermTracker.Services;

namespace WGUTermTracker.Views;

public partial class CourseStartDateReportPage : ContentPage
{
    private readonly SQLiteDatabaseService _sqliteDatabaseService;

    public CourseStartDateReportPage(SQLiteDatabaseService sqliteDatabaseService)
    {
        InitializeComponent();
        _sqliteDatabaseService = sqliteDatabaseService;

        // Default date range - current month
        var today = DateTime.Today;
        fromDatePicker.Date = new DateTime(today.Year, today.Month, 1);
        toDatePicker.Date = fromDatePicker.Date.AddMonths(1).AddDays(-1);
    }

    private async void OnRunReportClicked(object sender, EventArgs e)
    {
        DateTime fromDate = fromDatePicker.Date.Date;
        DateTime toDate = toDatePicker.Date.Date;

        if (fromDate > toDate)
        {
            await DisplayAlert("Error", "From date must be before To date.", "OK");
            return;
        }

        // Load Course Start Date Report Data
        var courses = await _sqliteDatabaseService.GetAllCoursesAsync();
        var terms = await _sqliteDatabaseService.GetAllTermsAsync();
        var termLookup = terms.ToDictionary(t => t.Id, t => t.Title);

        var filtered = courses
            .Where(c => c.StartDate.Date >= fromDate && c.StartDate.Date <= toDate)
            .OrderBy(c => c.StartDate)
            .Select(c => new CourseStartDateReportItem
            {
                Title = c.Title,
                Status = c.Status,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                TermTitle = termLookup.TryGetValue(c.TermId, out var tt) ? tt : "Unknown Term"
            })
            .ToList();

        resultsCollectionView.ItemsSource = filtered;
        resultsSummaryLabel.IsVisible = true;
        resultsSummaryLabel.Text = $"Found {filtered.Count} course(s) with StartDate between {fromDate:MM/dd/yyyy} and {toDate:MM/dd/yyyy}.";
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        resultsCollectionView.ItemsSource = null;
        resultsSummaryLabel.IsVisible = false;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}