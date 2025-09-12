using System.Collections.ObjectModel;
using WGUTermTracker.Services;
using WGUTermTracker.Models;

namespace WGUTermTracker.Views;

public partial class TermsPage : ContentPage
{
	private readonly SQLiteDatabaseService _sqliteDatabaseService;
    public ObservableCollection<Term> Terms { get; set; } = new ObservableCollection<Term>();
    Term _selectedTerm;
	public TermsPage(SQLiteDatabaseService sqliteDatabaseService)
	{
		InitializeComponent();
        _sqliteDatabaseService = sqliteDatabaseService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadAllTermsAsync();
        _selectedTerm = null;
    }

    async void OnAddTermClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddTermPage(_sqliteDatabaseService));
    }
    async void OnViewTermClicked(object sender, EventArgs e)
    {
        if (_selectedTerm != null)
        {
            await Navigation.PushAsync(new TermDetailsPage(_selectedTerm, _sqliteDatabaseService));
        }
        else
        {
            await DisplayAlert("Error", "Please select a Term to view.", "OK");
        }
    }

    async void OnUpdateTermClicked(object sender, EventArgs e)
    {
        if (_selectedTerm != null)
        {
            await Navigation.PushAsync(new UpdateTermPage(_selectedTerm, _sqliteDatabaseService));
        }
        else
        {
            await DisplayAlert("Error", "Please select a term to update.", "OK");
        }
    }

    private void OnTermSelected(object sender, SelectionChangedEventArgs e)
    {
        var selectedTerm = e.CurrentSelection.FirstOrDefault() as Term;

        if (selectedTerm != null)
        {
            _selectedTerm = selectedTerm;
        }
        else
        {
            _selectedTerm = null;           
        }
    }
    private async void OnDeleteTermClicked(object sender, EventArgs e)
    {
        if (_selectedTerm != null)
        {
            bool confirmTermDelete = await DisplayAlert("Confirm Term Delete", $"Are you sure you want to delete the selected term: '{_selectedTerm.Title}'?", "Yes", "No");
            if (confirmTermDelete)
            {
                await _sqliteDatabaseService.DeleteTermAsync(_selectedTerm.Id);

                // Reset selected term
                _selectedTerm = null;
            }
        }
        else
        {
            await DisplayAlert("Error", "Please select a term to deleted.", "OK");
        }

        // Reload Terms CollectionView
        await LoadAllTermsAsync();
        _selectedTerm = null;
    }

    private async Task LoadAllTermsAsync()
    {
        // Seed Testing Data
        await _sqliteDatabaseService.SeedTermDataAsync();

        // Clear Terms for new list of updated Terms
        Terms.Clear();

        // Create temp list
        var newTermsList = await _sqliteDatabaseService.GetAllTermsAsync();

        // Add newTermsList items to Terms ObservableCollection
        foreach (var term in newTermsList)
        {
            Terms.Add(term);
        }
    }
}