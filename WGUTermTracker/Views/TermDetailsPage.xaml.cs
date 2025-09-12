using WGUTermTracker.Services;
using WGUTermTracker.Models;

namespace WGUTermTracker.Views;

public partial class TermDetailsPage : ContentPage
{
    private readonly SQLiteDatabaseService _sqliteDatabaseService;
    private Term _term;
    private Course _selectedCourse;
    private List<Course> _allCourses;

    public TermDetailsPage(Term term, SQLiteDatabaseService sqliteDatabaseService)
    {
        InitializeComponent();
        _sqliteDatabaseService = sqliteDatabaseService;
        _term = term;

        // Display Term Details
        termTitleLabel.Text = $"{_term.Title}";
        termStartDateLabel.Text = $"Start Date: {_term.StartDate.ToString("d")}";
        termEndDateLabel.Text = $"End Date: {_term.EndDate.ToString("d")}";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAllCoursesAsync();
        _selectedCourse = null;
    }

    private async Task LoadAllCoursesAsync()
    {
        // Get Courses
        _allCourses = await _sqliteDatabaseService.GetAllCoursesAsync();

        // Filter Courses
        var termCourses = _allCourses.Where(c => c.TermId == _term.Id).ToList();

        // Bind Courses to CollectionView
        coursesCollectionView.ItemsSource = termCourses;
    }

    private void OnCourseSelected(object sender, SelectionChangedEventArgs e)
    {
        var selectedCourse = e.CurrentSelection.FirstOrDefault() as Course;

        if (selectedCourse != null)
        {
            _selectedCourse = selectedCourse;
        }
        else
        {
            _selectedCourse = null;
        }
    }

    private async void OnAddCourseButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddCoursePage(_term, _sqliteDatabaseService));
    }

    private async void OnViewCourseButtonClicked(object sender, EventArgs e)
    {
        if (_selectedCourse != null)
        {
            // Navigate to Course Details Page
            await Navigation.PushAsync(new CourseDetailsPage(_selectedCourse, _sqliteDatabaseService));
        }
        else
        {
            await DisplayAlert("Error", "Please select a course to view details.", "OK");
        }
    }

    private async void OnUpdateCourseButtonClicked(object sender, EventArgs e)
    {
        if (_selectedCourse != null)
        {
            await Navigation.PushAsync(new UpdateCoursePage(_term, _selectedCourse, _sqliteDatabaseService));
        }
        else
        {
            await DisplayAlert("Error", "Please select a course to update.", "OK");
        }
    }

    private async void OnDeleteCourseButtonClicked(object sender, EventArgs e)
    {
        if (_selectedCourse != null)
        {
            bool confirmCourseDelete = await DisplayAlert("Confirm Course Delete", $"Are you sure you want to delete the selected course: '{_selectedCourse.Title}'?", "Yes", "No");
            if (confirmCourseDelete)
            {
                await _sqliteDatabaseService.DeleteCourseAsync(_selectedCourse.Id);
                _selectedCourse = null;
            }
        }
        else
        {
            await DisplayAlert("Error", "Please select a course to delete.", "OK");
        }

        await LoadAllCoursesAsync();
        _selectedCourse = null;
    }

    private async void OnCourseStartDateReportButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CourseStartDateReportPage(_sqliteDatabaseService));
    }

    private async void OnCancelButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}