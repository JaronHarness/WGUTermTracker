using WGUTermTracker.Models;
using WGUTermTracker.Models.Enums;
using WGUTermTracker.Services;

namespace WGUTermTracker.Views;

public partial class UpdateTermPage : ContentPage
{
    private readonly SQLiteDatabaseService _sqliteDatabaseService;
    private Term _term;


    public UpdateTermPage(Term term, SQLiteDatabaseService sqliteDatabaseService)
    {
        InitializeComponent();
        _sqliteDatabaseService = sqliteDatabaseService;
        _term = term;
        BindingContext = _term;
    }

    private async void OnUpdateTermClicked(object sender, EventArgs e)
    {
        await UpdateTermAsync();
    }

    private async Task UpdateTermAsync()
    {
        // Term Property Variables
        var title = enteredTermTitle.Text.Trim();
        var startDate = startDatePicker.Date;
        var endDate = endDatePicker.Date;

        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlert("Error: ", "Please enter a Term title.", "OK");
            return;
        }

        if (!IsStartDateBeforeEndDate(startDate, endDate))
        {
            await DisplayAlert("Error: ", "Start Date must be before End Date.", "OK");
            return;
        }

        await _sqliteDatabaseService.UpdateTermAsync(_term.Id, title, startDate, endDate);
        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private bool IsStartDateBeforeEndDate(DateTime start, DateTime end)
    {
        if (start > end)
        {
            return false;
        }
        return true;
    }
}