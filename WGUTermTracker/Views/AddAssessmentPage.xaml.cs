using Plugin.LocalNotification;
using WGUTermTracker.Models;
using WGUTermTracker.Models.Enums;
using WGUTermTracker.Services;

namespace WGUTermTracker.Views;

public partial class AddAssessmentPage : ContentPage
{
    private readonly SQLiteDatabaseService _sqliteDatabaseService;
    private Course _course;

    public AddAssessmentPage(Course course, SQLiteDatabaseService sqliteDatabaseService)
	{
		InitializeComponent();
        _sqliteDatabaseService = sqliteDatabaseService;
        _course = course;

        // Populates the Assessment Type dropdown
        assessmentTypePicker.ItemsSource = Enum.GetValues(typeof(AssessmentType)).Cast<AssessmentType>().ToList();
        // Sets Default Assessment Type value
        assessmentTypePicker.SelectedItem = AssessmentType.Objective;
        _sqliteDatabaseService = sqliteDatabaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var courses = await _sqliteDatabaseService.GetAllCoursesAsync();
        coursePicker.ItemsSource = courses;

        // Set the selected course to the one passed in
        if (_course != null)
        {
            var matchingCourse = courses.FirstOrDefault(c => c.Id == _course.Id);
            coursePicker.SelectedItem = matchingCourse;
        }
        else
        {
            coursePicker.SelectedIndex = -1;
        }
    }

    private async void OnAddAssessmentClicked(object sender, EventArgs e)
    {
        await AddAssessmentAsync();
    }

    private async Task AddAssessmentAsync()
    {
        // Title
        string title = enteredAssessmentTitle.Text;

        if (IsInputStringNullOrEmptyValue(title))
        {
            await DisplayAlert("Error: ", "Please enter an Assessment title.", "OK");
            return;
        }
        else
        {
            title = enteredAssessmentTitle.Text.Trim();
        }

        // Course
        if (coursePicker.SelectedItem == null)
        {
            await DisplayAlert("Error: ", "Please select a Course for the Assessment.", "OK");
            return;
        }
        int courseId = ((Course)coursePicker.SelectedItem).Id;


        // AssessmentType
        AssessmentType type = (AssessmentType)assessmentTypePicker.SelectedItem;

        // Start and End Dates 
        DateTime startDate = assessmentStartDatePicker.Date;
        DateTime endDate = assessmentEndDatePicker.Date;
        if (!IsStartDateBeforeEndDate(startDate, endDate))
        {
            await DisplayAlert("Error: ", "Start Date must be before End Date.", "OK");
            return;
        }

        // Notifications
        bool enableNotifications = notificationsCheckBox.IsChecked;

        int assessmentId = await _sqliteDatabaseService.AddAssessmentAsync(title, type, startDate, endDate, enableNotifications, courseId);

        if(enableNotifications)
        {
            await StartDateNotification(assessmentId, title, startDate);
            await EndDateNotification(assessmentId, title, endDate);
        }

        await Navigation.PopAsync();
    }

    private async Task StartDateNotification(int assessmentId, string title, DateTime startDate)
    {
        var notifyTime = startDate.AddHours(8);
        if (notifyTime < DateTime.Now)
        {
            notifyTime = DateTime.Now;
        }

        var startNotification = new NotificationRequest
        {
            NotificationId = assessmentId * 10 + 1,
            Title = "Assessment Start Date Reminder",
            Description = $"Assessment '{title}' starts today!",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyTime,
                NotifyRepeatInterval = null
            },
        };
        await LocalNotificationCenter.Current.Show(startNotification);
    }

    private async Task EndDateNotification(int assessmentId, string title, DateTime endDate)
    {
        var notifyTime = endDate.AddHours(8);
        if (notifyTime < DateTime.Now)
        {
            notifyTime = DateTime.Now;
        }

        var endNotification = new NotificationRequest
        {
            NotificationId = assessmentId * 10 + 2,
            Title = "Assessment End Date Reminder",
            Description = $"Assessment '{title}' ends today!",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyTime,
                NotifyRepeatInterval = null
            },
        };
        await LocalNotificationCenter.Current.Show(endNotification);
    }

    private bool IsInputStringNullOrEmptyValue(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return true;
        }

        return false;
    }

    private bool IsStartDateBeforeEndDate(DateTime start, DateTime end)
    {
        if (start > end)
        {
            return false;
        }
        return true;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}