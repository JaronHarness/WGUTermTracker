using WGUTermTracker.Services;
using WGUTermTracker.Models;
using WGUTermTracker.Models.Enums;
using Plugin.LocalNotification;

namespace WGUTermTracker.Views;

public partial class UpdateCoursePage : ContentPage
{
	private readonly SQLiteDatabaseService _sqliteDatabaseService;
    private Term _term;
    private Course _course;

    public UpdateCoursePage(Term term, Course course,SQLiteDatabaseService sqliteDatabaseService)
	{
		InitializeComponent();
		_sqliteDatabaseService = sqliteDatabaseService;
        _term = term;
        _course = course;
		BindingContext = _course;

        // Populates the Status Picker dropdown
        statusPicker.ItemsSource = Enum.GetValues(typeof(CourseStatus)).Cast<CourseStatus>().ToList();
        // Sets Default Status value
        statusPicker.SelectedItem = _course.Status;

        notificationsCheckBox.IsChecked = _course.EnableNotifications;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var terms = await _sqliteDatabaseService.GetAllTermsAsync();
        termPicker.ItemsSource = terms;

        // Set the selected term to the one passed in
        if (_term != null)
        {
            var matchingTerm = terms.FirstOrDefault(t => t.Id == _term.Id);
            termPicker.SelectedItem = matchingTerm;
        }
        else
        {
            termPicker.SelectedIndex = -1;
        }
    }

    private async void OnUpdateCourseClicked(object sender, EventArgs e)
	{
		await UpdateCourseAsync();
    }

	private async Task UpdateCourseAsync()
	{
        string title = enteredCourseTitle.Text;

        if (IsInputStringNullOrEmptyValue(title))
        {
            await DisplayAlert("Error: ", "Please enter a Course title.", "OK");
            return;
        }
        else
        {
            title = enteredCourseTitle.Text.Trim();
        }

        if (termPicker.SelectedItem == null)
        {
            await DisplayAlert("Error: ", "Please select a Term for the Course.", "OK");
            return;
        }

        int termIdString = ((Term)termPicker.SelectedItem).Id;
        CourseStatus status = (CourseStatus)statusPicker.SelectedItem;

        string notes = enteredCourseNotes.Text;
        if (IsInputStringNullOrEmptyValue(notes))
        {
            notes = "";
        }
        else
        {
            notes = enteredCourseNotes.Text.Trim();
        }

        bool enableNotifications = notificationsCheckBox.IsChecked;

        DateTime startDate = courseStartDatePicker.Date;
        DateTime endDate = courseEndDatePicker.Date;
        if (!IsStartDateBeforeEndDate(startDate, endDate))
        {
            await DisplayAlert("Error: ", "Start Date must be before End Date.", "OK");
            return;
        }

        string instructorName = enteredInstructorName.Text;
        if (IsInputStringNullOrEmptyValue(instructorName))
        {
            await DisplayAlert("Error: ", "Please enter the instuctor's name.", "OK");
            return;
        }
        else
        {
            instructorName = enteredInstructorName.Text.Trim();
        }

        string instructorPhone = enteredInstructorPhone.Text;
        if (IsInputStringNullOrEmptyValue(instructorPhone))
        {
            await DisplayAlert("Error: ", "Please enter the instructor's phone number.", "OK");
            return;
        }
        else
        {
            instructorPhone = enteredInstructorPhone.Text.Trim();
        }

        string instructorEmail = enteredInstructorEmail.Text;
        if (IsInputStringNullOrEmptyValue(instructorEmail))
        {
            await DisplayAlert("Error: ", "Please enter the instuctor's email address.", "OK");
            return;
        }
        else
        {
            instructorEmail = enteredInstructorEmail.Text.Trim();

        }

        await _sqliteDatabaseService.UpdateCourseAsync(_course.Id, title, status, startDate, endDate, notes, enableNotifications, instructorName, instructorPhone, instructorEmail, termIdString);

        // Cancel old notifications
        LocalNotificationCenter.Current.Cancel(new[] {_course.Id * 10 + 1});
        LocalNotificationCenter.Current.Cancel(new[] { _course.Id * 10 + 2 });

        // If enabled create new notifications
        if (enableNotifications)
        {
            await StartDateNotification(_course.Id, title, startDate);
            await EndDateNotification(_course.Id, title, endDate);
        }

        await Navigation.PopAsync();
    }

    private async Task StartDateNotification(int courseId, string title, DateTime startDate)
    {
        var notifyTime = startDate.AddHours(8);

        if (notifyTime < DateTime.Now)
        {
            notifyTime = DateTime.Now;
        }

        var startNotification = new NotificationRequest
        {
            NotificationId = courseId * 10 + 1,
            Title = "Course Start Date Reminder",
            Description = $"Course '{title}' starts today!",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyTime,
                NotifyRepeatInterval = null
            },
        };
        await LocalNotificationCenter.Current.Show(startNotification);
    }

    private async Task EndDateNotification(int courseId, string title, DateTime endDate)
    {
        var notifyTime = endDate.AddHours(8);
        if (notifyTime < DateTime.Now)
        {
            notifyTime = DateTime.Now;
        }

        var endNotification = new NotificationRequest
        {
            NotificationId = courseId * 10 + 2,
            Title = "Course End Date Reminder",
            Description = $"Course '{title}' ends today!",
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