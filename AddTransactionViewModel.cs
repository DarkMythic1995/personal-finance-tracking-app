using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker.Views;
using System.Collections.ObjectModel;

namespace PersonalFinanceTracker.ViewModels
{
    public partial class AddTransactionViewModel : ObservableObject
    {
        /// <summary>
        /// Service for data operations (e.g., saving transactions to a database)
        /// </summary>
        private readonly DataService _dataService;

        /// <summary>
        /// Service for checking network connectivity.
        /// </summary>
        private readonly IConnectivity _connectivity;

        /// <summary>
        /// Reference to the main view model for refreshing data.
        /// </summary>
        private readonly MainViewModel _mainViewModel;

        /// <summary>
        /// Dependency injection container for resolving services.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Observable property for the transaction being added.
        /// </summary>
        [ObservableProperty]
        private Transaction selectedTransaction;

        /// <summary>
        /// Observable collection of transaction categories for the UI dropdown.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> categories = new() { "Groceries", "Transport", "Salary", "Dining Out", "Entertainment", "Utilities" };

        /// <summary>
        /// Initializes a new instance of the AddTransactionViewModel with required dependencies.
        /// Sets up a new transaction and predefined categories.
        /// </summary>
        /// <param name="dataService">Service for data operations.</param>
        /// <param name="connectivity">Service for network connectivity checks.</param>
        /// <param name="mainViewModel">Main view model for refreshing data.</param>
        /// <param name="serviceProvider">DI container for resolving services.</param>
        public AddTransactionViewModel(DataService dataService, IConnectivity connectivity, MainViewModel mainViewModel, IServiceProvider serviceProvider)
        {
            // Validate and assign the data service
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            // Validate and assign the connectivity service
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            // Validate and assign the main view model
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            // Validate and assign the service provider
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            // Initialize a new transaction with default values
            SelectedTransaction = new Transaction { Id = Guid.NewGuid(), Date = DateTime.Today, IsIncome = false };
        }

        /// <summary>
        /// Saves the transaction to the database and navigates to its detail page.
        /// </summary>
        [RelayCommand]
        private async Task Save()
        {
            // Validate transaction amount and category
            if (SelectedTransaction.Amount <= 0 || string.IsNullOrEmpty(SelectedTransaction.Category))
            {
                // Display error if validation fails
                await Shell.Current.DisplayAlert("Error", "Please enter a valid amount and category.", "OK");
                return;
            }
            // Check for internet connectivity
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // Display error if no internet connection
                await Shell.Current.DisplayAlert("No Internet", "An internet connection is required to save.", "OK");
                return;
            }
            // Log transaction details for debugging
            System.Diagnostics.Debug.WriteLine($"Saving transaction: {SelectedTransaction.Category}, {SelectedTransaction.Amount}, {SelectedTransaction.Date}, IsIncome: {SelectedTransaction.IsIncome}");
            // Save the transaction to the database
            await _dataService.AddTransactionAsync(SelectedTransaction);
            // Log success for debugging
            System.Diagnostics.Debug.WriteLine("Transaction saved to database");
            // Refresh the main view model's data
            await _mainViewModel.InitializeAsync();
            // Log refresh for debugging
            System.Diagnostics.Debug.WriteLine("MainViewModel refreshed");

            // Resolve DetailViewModel using dependency injection
            var detailViewModel = _serviceProvider.GetRequiredService<DetailViewModel>();
            // Set the transaction ID for the detail page
            detailViewModel.TransactionId = SelectedTransaction.Id.ToString();
            // Create a new detail page with the resolved view model
            var detailPage = new DetailPage(detailViewModel);
            // Navigate to the detail page
            await Shell.Current.Navigation.PushAsync(detailPage);
        }

        /// <summary>
        /// Cancels the transaction and navigates back to the main page.
        /// </summary>
        [RelayCommand]
        private async Task Cancel()
        {
            // Navigate back to the main page using Shell navigation
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}