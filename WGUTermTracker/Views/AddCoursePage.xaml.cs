using WGUTermTracker.Models;
using WGUTermTracker.Models.Enums;
using WGUTermTracker.Services;
using Plugin.LocalNotification;
using System.Text.RegularExpressions; // For Email Validation

namespace WGUTermTracker.Views;

public partial class AddCoursePage : ContentPage
{
    private readonly SQLiteDatabaseService _sqliteDatabaseService;
    private Term _term;

    // Industry-standard security feature - email validation
    private static readonly Regex EmailRegex = new(
        @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,24}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public AddCoursePage(Term term, SQLiteDatabaseService sqliteDatabaseService)
    {
        InitializeComponent();
        _sqliteDatabaseService = sqliteDatabaseService;
        _term = term;

        statusPicker.ItemsSource = Enum.GetValues(typeof(CourseStatus)).Cast<CourseStatus>().ToList();
        statusPicker.SelectedItem = CourseStatus.InActive;
        _sqliteDatabaseService = sqliteDatabaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var terms = await _sqliteDatabaseService.GetAllTermsAsync();
        termPicker.ItemsSource = terms;

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

    private async void OnAddCourseClicked(object sender, EventArgs e)
    {
        await AddCourseAsync();
    }

    private async Task AddCourseAsync()
    {
        // Title
        string title = enteredCourseTitle.Text;
        if (IsInputStringNullOrEmptyValue(title))
        {
            await DisplayAlert("Error: ", "Please enter a Course title.", "OK");
            return;
        }
        title = title.Trim();

        // Term
        if (termPicker.SelectedItem == null)
        {
            await DisplayAlert("Error: ", "Please select a Term for the Course.", "OK");
            return;
        }
        int termId = ((Term)termPicker.SelectedItem).Id;

        // CourseStatus
        CourseStatus status = (CourseStatus)statusPicker.SelectedItem;

        // Notes
        string notes = enteredCourseNotes.Text;
        notes = IsInputStringNullOrEmptyValue(notes) ? "" : notes.Trim();

        // Notifications
        bool enableNotifications = notificationsCheckBox.IsChecked;

        // Dates
        DateTime startDate = courseStartDatePicker.Date;
        DateTime endDate = courseEndDatePicker.Date;
        if (!IsStartDateBeforeEndDate(startDate, endDate))
        {
            await DisplayAlert("Error: ", "Start Date must be before End Date.", "OK");
            return;
        }

        // Instructor Name
        string instructorName = enteredInstructorName.Text;
        if (IsInputStringNullOrEmptyValue(instructorName))
        {
            await DisplayAlert("Error: ", "Please enter the instuctor's name.", "OK");
            return;
        }
        instructorName = instructorName.Trim();

        // Instructor Phone
        string instructorPhone = enteredInstructorPhone.Text;
        if (IsInputStringNullOrEmptyValue(instructorPhone))
        {
            await DisplayAlert("Error: ", "Please enter the instructor's phone number.", "OK");
            return;
        }
        instructorPhone = instructorPhone.Trim();

        // Instructor Email (validation + sanitization)
        string instructorEmail = enteredInstructorEmail.Text;
        if (IsInputStringNullOrEmptyValue(instructorEmail))
        {
            await DisplayAlert("Error: ", "Please enter the instuctor's email address.", "OK");
            return;
        }
        instructorEmail = instructorEmail.Trim();

        if (!IsValidEmail(instructorEmail))
        {
            await DisplayAlert("Error", "Please enter a valid instructor email (e.g., instructorname@campusname.edu).", "OK");
            return;
        }

        int courseId = await _sqliteDatabaseService.AddCourseAsync(
            title,
            status,
            startDate,
            endDate,
            notes,
            enableNotifications,
            instructorName,
            instructorPhone,
            instructorEmail,
            termId);

        if (enableNotifications)
        {
            await StartDateNotification(courseId, title, startDate);
            await EndDateNotification(courseId, title, endDate);
        }

        await Navigation.PopAsync();
    }

    // Email validation helper (Industry Standard Security feature)
    private bool IsValidEmail(string email)
    {
        if (email.Length > 120) return false;
        if (email.Contains('\n') || email.Contains('\r')) return false;
        return EmailRegex.IsMatch(email);
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

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private bool IsInputStringNullOrEmptyValue(string input)
    {
        return string.IsNullOrEmpty(input);
    }

    private bool IsStartDateBeforeEndDate(DateTime start, DateTime end)
    {
        return start <= end;
    }
}