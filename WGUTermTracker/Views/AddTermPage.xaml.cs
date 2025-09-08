using WGUTermTracker.Models;
using WGUTermTracker.Services;

namespace WGUTermTracker.Views;

public partial class AddTermPage : ContentPage
{
	private readonly SQLiteDatabaseService _sqliteDatabaseService;
 
	public AddTermPage(SQLiteDatabaseService sqliteDatabaseService)
	{
		InitializeComponent();

		_sqliteDatabaseService = sqliteDatabaseService;
	}

	private async void OnAddTermClicked(object sender, EventArgs e)
	{
		await AddTermAsync();
	}

	private async Task AddTermAsync()
	{
		// Term Property Variables
		var title = enteredTermTitle.Text.Trim();
		var startDate = startDatePicker.Date;
		var endDate = endDatePicker.Date;

		if (IsInputStringNullOrEmptyValue(title))
		{
			await DisplayAlert("Error: ", "Please enter a Term title.", "OK");
			return;
		}

		if (!IsStartDateBeforeEndDate(startDate, endDate))
		{
			await DisplayAlert("Error: ", "Start Date must be before End Date.", "OK");
			return;
		}

		await _sqliteDatabaseService.AddTermAsync(title, startDate, endDate);
        await Navigation.PopAsync();

    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
		await Navigation.PopAsync();
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
}