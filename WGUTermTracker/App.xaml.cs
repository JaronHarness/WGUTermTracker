using WGUTermTracker.Services;
using WGUTermTracker.Views;

namespace WGUTermTracker;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
  
        var sqliteDatabaseService = new SQLiteDatabaseService();

        // Uncomment the line below to clear DB tables and reseed test data.
        //Task.Run(async () => await sqliteDatabaseService.ClearTablesAsync()).Wait();

        // Seeds Term, Course, and Assessment data into the DB
        Task.Run(async () => await sqliteDatabaseService.SeedTermDataAsync()).Wait();

        MainPage = new NavigationPage(new TermsPage(sqliteDatabaseService));
    }
}
