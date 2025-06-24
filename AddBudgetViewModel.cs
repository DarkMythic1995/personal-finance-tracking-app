using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel responsible for managing the addition of budgets in the Personal Finance Tracker application.
/// </summary>
namespace PersonalFinanceTracker.ViewModels
{
    /// <summary>
    /// A ViewModel class that provides the logic for adding a new budget entry.
    /// Includes observable properties for data binding and relay commands for save and cancel actions.
    /// </summary>
    public partial class AddBudgetViewModel : ObservableObject
    {
        /// <summary>
        /// Service for database operations.
        /// </summary>
        private readonly DataService _dataService;

        /// <summary>
        /// Service for checking network connectivity.
        /// </summary>
        private readonly IConnectivity _connectivity;

        /// <summary>
        /// Main view model for refreshing data after changes.
        /// </summary>
        private readonly MainViewModel _mainViewModel;

        /// <summary>
        /// Observable property for the budget being created
        /// </summary>
        [ObservableProperty]
        private Budget budget;

        /// <summary>
        /// Observable collection of predefined budget categories for UI selection.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> categories = new() { "Groceries", "Transport", "Dining Out", "Entertainment", "Utilities" };

        /// <summary>
        /// Initializes a new instance of the AddBudgetViewModel with required services.
        /// Sets up the DataService, connectivity, and MainViewModel dependencies, and initializes a new Budget object.
        /// </summary>
        /// <param name="dataService">The service for database operations.</param>
        /// <param name="connectivity">The service for checking network connectivity.</param>
        /// <param name="mainViewModel">The main view model for refreshing data after changes.</param>
        public AddBudgetViewModel(DataService dataService, IConnectivity connectivity, MainViewModel mainViewModel)
        {
            // Assign the data service
            _dataService = dataService;
            // Assign the connectivity service (currently unused)
            _connectivity = connectivity;
            // Assign the main view model
            _mainViewModel = mainViewModel;
            // Initialize a new budget with a unique ID and current month
            Budget = new Budget { Id = Guid.NewGuid(), Month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) };
        }

        /// <summary>
        /// Saves the budget if validation passes, then navigates back to the MainPage.
        /// Checks for a valid amount and category, displays an error if invalid, and refreshes the main view model.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [RelayCommand]
        private async Task Save()
        {
            // Validate that the budget amount is positive and a category is selected
            if (Budget.Amount <= 0 || string.IsNullOrEmpty(Budget.Category))
            {
                // Display an error alert if validation fails
                await Shell.Current.DisplayAlert("Error", "Please enter a valid amount and category.", "OK");
                return;
            }
            // Save the budget to the database
            await _dataService.AddBudgetAsync(Budget);
            // Refresh the main view model's data
            await _mainViewModel.InitializeAsync();
            // Navigate back to the MainPage using Shell navigation
            await Shell.Current.GoToAsync("//MainPage");
        }

        /// <summary>
        /// Cancels the budget addition and navigates back to the MainPage.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [RelayCommand]
        private async Task Cancel()
        {
            // Navigate back to the MainPage using Shell navigation
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}