using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PersonalFinanceTracker.ViewModels
{
    /// <summary>
    /// View model for the main page, managing transactions, budgets, and exchange rate data.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        /// <summary>
        /// Static instance for singleton-like access.
        /// </summary>
        private static MainViewModel _instance;

        /// <summary>
        /// Service for database operations.
        /// </summary>
        private readonly DataService _dataService;

        /// <summary>
        /// Service for network connectivity checks.
        /// </summary>
        private readonly IConnectivity _connectivity;

        /// <summary>
        /// Service for fetching exchange rates.
        /// </summary>
        private readonly IApiService _apiService;

        /// <summary>
        /// Observable collection of transactions for UI binding.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Transaction> transactions = new();

        /// <summary>
        /// Observable collection of budgets for UI binding.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Budget> budgets = new();

        /// <summary>
        /// Current month for budget calculations (first day of the month).
        /// </summary>
        [ObservableProperty]
        private DateTime currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        /// <summary>
        /// Text displaying the USD-to-JPY exchange rate or error messages.
        /// </summary>
        [ObservableProperty]
        private string exchangeRateText = "Fetching exchange rate...";

        /// <summary>
        /// Initializes a new instance of MainViewModel with required dependencies.
        /// </summary>
        /// <param name="dataService">Service for database operations.</param>
        /// <param name="connectivity">Service for network connectivity checks.</param>
        /// <param name="apiService">Service for fetching exchange rates.</param>
        public MainViewModel(DataService dataService, IConnectivity connectivity, IApiService apiService)
        {
            // Validate and assign data service
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));

            // Validate and assign connectivity service
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));

            // Validate and assign API service
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            // Initialize transactions collection
            transactions = new ObservableCollection<Transaction>();

            // Initialize budgets collection
            budgets = new ObservableCollection<Budget>();

            // Set the static instance to this object
            _instance = this;
        }

        /// <summary>
        /// Provides access to the singleton instance of MainViewModel (with a risky fallback).
        /// </summary>
        /// <returns>The MainViewModel instance.</returns>
        public static MainViewModel GetInstance()
        {
            // Return existing instance or create a new one with null dependencies.
            return _instance ??= new MainViewModel(null, null, null);
        }

        /// <summary>
        /// Initializes the view model by setting up the database, loading data, and fetching exchange rates.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            try
            {
                // Log initialization start
                Debug.WriteLine("MainViewModel: Starting initialization");

                // Run initialization tasks asynchronously
                await Task.Run(async () =>
                {
                    // Initialize the database
                    await _dataService.InitializeDatabaseAsync();
                    // Load transactions and budgets
                    await LoadDataAsync();
                    // Fetch exchange rate
                    await LoadExchangeRateAsync();
                });

                // Log initialization completion
                Debug.WriteLine("MainViewModel: Initialization completed");
            }
            catch (Exception ex)
            {
                // Log error details
                Debug.WriteLine($"InitializeAsync error: {ex.Message}\nStackTrace: {ex.StackTrace}");

                // Display error to user
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to initialize the app.", "OK");
            }
        }

        /// <summary>
        /// Loads transactions and budgets from the database, updating observable collections.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadDataAsync()
        {
            try
            {
                // Log transaction loading start
                Debug.WriteLine("Loading transactions...");

                // Retrieve transactions from the database
                var transactionList = await _dataService.GetTransactionsAsync();

                // Update transactions on the main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Clear existing transactions
                    Transactions.Clear();

                    // Add transactions in descending date order
                    foreach (var transaction in transactionList.OrderByDescending(t => t.Date))
                    {
                        Transactions.Add(transaction);
                    }

                    // Log the number of loaded transactions
                    Debug.WriteLine($"Loaded {Transactions.Count} transactions");
                });

                // Log budget loading start
                Debug.WriteLine("Loading budgets...");

                // Retrieve budgets from the database
                var budgetList = await _dataService.GetBudgetsAsync();

                // Update budgets on the main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Clear existing budgets
                    Budgets.Clear();
                    // Add budgets to the collection
                    foreach (var budget in budgetList)
                    {
                        Budgets.Add(budget);
                    }
                    // Log the number of loaded budgets
                    Debug.WriteLine($"Loaded {Budgets.Count} budgets");
                });
            }
            catch (Exception ex)
            {
                // Log error details
                Debug.WriteLine($"LoadDataAsync error: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Fetches the USD-to-JPY exchange rate and updates the UI.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadExchangeRateAsync()
        {
            // Check for internet connectivity
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // Update UI with error message
                ExchangeRateText = "No internet connection.";
                // Log connectivity issue
                Debug.WriteLine("No internet connection for exchange rate");
                return;
            }

            try
            {
                // Fetch exchange rate from API
                var rate = await _apiService.GetExchangeRateAsync("USD", "JPY");
                // Update UI on the main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Format and display the exchange rate
                    ExchangeRateText = $"1 USD = {rate:F2} JPY";
                });
            }
            catch (Exception ex)
            {
                // Update UI with error message on the main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ExchangeRateText = "Failed to load exchange rate.";
                });
                // Log error details
                Debug.WriteLine($"LoadExchangeRateAsync error: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Navigates to the AddTransactionPage.
        /// </summary>
        [RelayCommand]
        private async Task AddTransaction()
        {
            // Log command execution
            Debug.WriteLine("AddTransactionCommand executed");
            // Navigate to the AddTransactionPage using Shell navigation
            await Shell.Current.GoToAsync("//AddTransactionPage");
        }

        /// <summary>
        /// Navigates to the DetailPage for a specific transaction.
        /// </summary>
        /// <param name="transaction">The transaction to view.</param>
        [RelayCommand]
        private async Task ViewTransaction(Transaction transaction)
        {
            // Log command execution with transaction ID
            Debug.WriteLine($"ViewTransactionCommand executed for ID: {transaction?.Id}");
            // Navigate to the DetailPage with the transaction ID as a query parameter
            await Shell.Current.GoToAsync($"//DetailPage?TransactionId={transaction.Id}");
        }

        /// <summary>
        /// Deletes a transaction from the database and updates the UI.
        /// </summary>
        /// <param name="transaction">The transaction to delete.</param>
        [RelayCommand]
        private async Task DeleteTransaction(Transaction transaction)
        {
            // Check for null transaction
            if (transaction == null)
            {
                // Log null transaction error
                Debug.WriteLine("DeleteTransaction: Transaction is null");
                return;
            }

            // Check for internet connectivity
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // Display connectivity error
                await Application.Current.MainPage.DisplayAlert("No Internet", "You need an internet connection to delete transactions.", "OK");
                return;
            }

            try
            {
                // Log deletion attempt
                Debug.WriteLine($"Deleting transaction with Id: {transaction.Id}");
                // Delete transaction from the database
                await _dataService.DeleteTransactionAsync(transaction.Id);
                // Remove transaction from the observable collection
                Transactions.Remove(transaction);
                // Log successful deletion
                Debug.WriteLine($"Transaction with Id {transaction.Id} deleted successfully");
            }
            catch (Exception ex)
            {
                // Display error to user
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete transaction: {ex.Message}", "OK");
                // Log error details
                Debug.WriteLine($"DeleteTransaction error: {ex}");
            }
        }

        /// <summary>
        /// Navigates to the AddBudgetPage.
        /// </summary>
        [RelayCommand]
        private async Task AddBudget()
        {
            // Log command execution
            Debug.WriteLine("AddBudgetCommand executed");
            // Navigate to the AddBudgetPage using Shell navigation
            await Shell.Current.GoToAsync("//AddBudgetPage");
        }

        /// <summary>
        /// Navigates to the ReportsPage.
        /// </summary>
        [RelayCommand]
        private async Task ViewReports()
        {
            // Log command execution
            Debug.WriteLine("ViewReportsCommand executed");
            // Navigate to the ReportsPage using Shell navigation
            await Shell.Current.GoToAsync("//ReportsPage");
        }

        /// <summary>
        /// Navigates to the EditTransactionPage for a specific transaction.
        /// </summary>
        /// <param name="transaction">The transaction to edit.</param>
        [RelayCommand]
        private async Task EditTransaction(Transaction transaction)
        {
            // Log command execution with transaction ID
            Debug.WriteLine($"EditTransactionCommand executed for ID: {transaction?.Id}");
            // Navigate to the EditTransactionPage with the transaction ID as a query parameter
            await Shell.Current.GoToAsync($"//EditTransactionPage?TransactionId={transaction.Id}");
        }

        /// <summary>
        /// Displays an action sheet for transaction options (edit or view details).
        /// </summary>
        /// <param name="transaction">The transaction to act on.</param>
        [RelayCommand]
        private async Task ShowTransactionOptions(Transaction transaction)
        {
            // Log command execution with transaction ID
            Debug.WriteLine($"ShowTransactionOptionsCommand executed for ID: {transaction?.Id}");
            // Display action sheet with options
            string action = await Application.Current.MainPage.DisplayActionSheet(
                "Choose an action",
                "Cancel",
                null,
                "Edit Details",
                "View Details");

            // Handle selected action
            switch (action)
            {
                case "Edit Details":
                    // Navigate to edit page
                    await EditTransaction(transaction);
                    break;
                case "View Details":
                    // Navigate to view page
                    await ViewTransaction(transaction);
                    break;
            }
        }

        /// <summary>
        /// Deletes a budget from the database and updates the UI, with user confirmation.
        /// </summary>
        /// <param name="budget">The budget to delete.</param>
        [RelayCommand]
        private async Task DeleteBudget(Budget budget)
        {
            // Check for null budget
            if (budget == null)
            {
                // Log null budget error
                Debug.WriteLine("DeleteBudget: Budget is null");
                return;
            }

            // Check for internet connectivity
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // Display connectivity error
                await Application.Current.MainPage.DisplayAlert("No Internet", "You need an internet connection to delete budgets.", "OK");
                return;
            }

            // Prompt user to confirm deletion
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Deletion",
                $"Are you sure you want to delete the budget for {budget.Category}? This action cannot be undone.",
                "Yes",
                "No");

            // Exit if user cancels
            if (!confirm) return;

            try
            {
                // Log deletion attempt
                Debug.WriteLine($"Deleting budget with Id: {budget.Id}");
                // Delete budget from the database
                await _dataService.DeleteBudgetAsync(budget.Id);
                // Find budget in the collection
                var budgetToRemove = Budgets.FirstOrDefault(b => b.Id == budget.Id);
                if (budgetToRemove != null)
                {
                    // Remove budget from the observable collection
                    Budgets.Remove(budgetToRemove);
                }
                else
                {
                    // Log if budget not found
                    Debug.WriteLine($"Budget with Id {budget.Id} not found");
                }
            }
            catch (Exception ex)
            {
                // Display error to user
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete budget: {ex.Message}", "OK");
                // Log error details
                Debug.WriteLine($"DeleteBudget error: {ex}");
            }
        }

        /// <summary>
        /// Calculates the spending progress for a budget category in the current month.
        /// </summary>
        /// <param name="category">The budget category.</param>
        /// <returns>The progress percentage (0-100).</returns>
        public async Task<decimal> GetBudgetProgressAsync(string category)
        {
            // Find budget for the specified category and month
            var budget = Budgets.FirstOrDefault(b => b.Category == category &&
                                                    b.Month.Year == CurrentMonth.Year &&
                                                    b.Month.Month == CurrentMonth.Month);
            if (budget == null)
            {
                // Log if no budget found
                Debug.WriteLine($"GetBudgetProgressAsync: No budget found for {category}");
                return 0;
            }
            // Get total spending for the category
            var spent = await _dataService.GetSpendingForCategoryAsync(category, CurrentMonth);
            // Calculate progress percentage, capped at 100%
            var progress = budget.Amount == 0 ? 0 : Math.Min(100, (spent / budget.Amount) * 100);
            // Log budget progress details
            Debug.WriteLine($"GetBudgetProgressAsync: {category} - Budget: {budget.Amount}, Spent: {spent}, Progress: {progress}%");
            // Return the progress percentage
            return progress;
        }
    }
}