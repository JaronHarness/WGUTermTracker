using WGUTermTracker.Services;
using WGUTermTracker.Models;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace WGUTermTracker.Views;

public partial class CourseDetailsPage : ContentPage
{
	private readonly SQLiteDatabaseService _sqliteDatabaseService;
	private Course _course;
	private Assessment _selectedAssessment;
	private List<Assessment> _allAssessments;

    public CourseDetailsPage(Course course, SQLiteDatabaseService sqliteDatabaseService)
	{
		InitializeComponent();
		_sqliteDatabaseService = sqliteDatabaseService;
		_course = course;

        // Display Course Details
		courseTitleLabel.Text = _course.Title;
		courseStartDateValue.Text = _course.StartDate.ToString("d");
		courseEndDateValue.Text = _course.EndDate.ToString("d");
		courseStatusValue.Text = _course.Status.ToString();
		courseNotesValue.Text = _course.Notes;
		courseEnableNotificationsCheckBox.IsChecked = _course.EnableNotifications;
        courseInstructorNameLabel.Text = $"{_course.InstructorName}";
		courseInstructorPhoneLabel.Text = $"{_course.InstructorPhone}";
		courseInstructorEmailLabel.Text = $"{_course.InstructorEmail}";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAllAssessmentsAsync();
        _selectedAssessment = null;
    }
    private async Task LoadAllAssessmentsAsync()
    {
        // Get All
        _allAssessments = await _sqliteDatabaseService.GetAllAssessmentsAsync();
        // Filter
        var courseAssessments = _allAssessments.Where(a => a.CourseId == _course.Id).ToList();
        // Bind Assessments to CollectionView
        assessmentsCollectionView.ItemsSource = courseAssessments;
    }
    private void OnAssessmentSelected(object sender, SelectionChangedEventArgs e)
    {
        var selectedAssessment = e.CurrentSelection.FirstOrDefault() as Assessment;
        if (selectedAssessment != null)
        {
            _selectedAssessment = selectedAssessment;
        }
        else
        {
            _selectedAssessment = null;
        }
    }
    private async void OnAddAssessmentButtonClicked(object sender, EventArgs e)
    {
       await Navigation.PushAsync(new AddAssessmentPage(_course, _sqliteDatabaseService));
    }
    private async void OnViewAssessmentButtonClicked(object sender, EventArgs e)
    {
        if (_selectedAssessment != null)
        {
            // Navigate to Assessment Details Page
            await Navigation.PushAsync(new AssessmentDetailsPage(_selectedAssessment, _sqliteDatabaseService));
        }
        else
        {
            await DisplayAlert("Error", "Please select an assessment to view.", "OK");
        }
    }
    private async void OnUpdateAssessmentButtonClicked(object sender, EventArgs e)
    {
        if (_selectedAssessment != null)
        {
            await Navigation.PushAsync(new UpdateAssessmentPage(_course, _selectedAssessment, _sqliteDatabaseService));
        }
        else
        {
            await DisplayAlert("Error", "Please select an assessment to update.", "OK");
        }
    }
    private async void OnDeleteAssessmentButtonClicked(object sender, EventArgs e)
    {
        if (_selectedAssessment != null)
        {
            var confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the selected term: '{_selectedAssessment.Title}'?", "Yes", "No");
            if (confirm)
            {
                await _sqliteDatabaseService.DeleteAssessmentAsync(_selectedAssessment.Id);
                await LoadAllAssessmentsAsync();
            }
        }
        else
        {
            await DisplayAlert("Error", "Please select an assessment to delete.", "OK");
        }
    }
    private async void OnCancelButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnShareNotesButtonClicked(object sender, EventArgs e)
    {
        ShareText(_course.Notes);
    }
    public async Task ShareText(string notes)
    {
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Text = notes,
            Title = $"Notes for {_course.Title}"
        });
    }
}