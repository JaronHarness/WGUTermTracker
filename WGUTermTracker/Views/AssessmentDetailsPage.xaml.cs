using WGUTermTracker.Models;
using WGUTermTracker.Services;

namespace WGUTermTracker.Views;

public partial class AssessmentDetailsPage : ContentPage
{
    private readonly SQLiteDatabaseService _sqliteDatabaseService;
    private Assessment _assessment;

    public AssessmentDetailsPage(Assessment assessment, SQLiteDatabaseService sqliteDatabaseService)
    {
        InitializeComponent();
        _sqliteDatabaseService = sqliteDatabaseService;
        _assessment = assessment;

        // Display Assessment Details
        assessmentTitleLabel.Text = _assessment.Title;
        assessmentTypeLabel.Text = $"{_assessment.Type}";
        assessmentStartDateLabel.Text = $"{_assessment.StartDate.ToString("d")}";
        assessmentEndDateLabel.Text = $"{_assessment.EndDate.ToString("d")}";
        assessmentEnableNotificationsCheckBox.IsChecked = _assessment.EnableNotifications;
    }

    private async void OnCancelButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}