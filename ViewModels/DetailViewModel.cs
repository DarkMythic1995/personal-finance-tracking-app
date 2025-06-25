using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinanceTracker.Services;

/// <summary>
/// ViewModel responsible for managing the details of a specific transaction in the Personal Finance Tracker application.
/// </summary>
namespace PersonalFinanceTracker.ViewModels
{
    /// <summary>
    /// A ViewModel class that provides the logic for displaying and managing transaction details.
    /// </summary>
    [QueryProperty(nameof(TransactionId), "TransactionId")]
    public partial class DetailViewModel : ObservableObject
    {
        /// <summary>
        /// Service for database operations.
        /// </summary>
        private readonly DataService _dataService;

        /// <summary>
        /// Observable property for the transaction's unique identifier.
        /// </summary>
        [ObservableProperty]
        private string _transactionId;

        /// <summary>
        /// Observable property for the transaction category.
        /// </summary>
        [ObservableProperty]
        private string category;

        /// <summary>
        /// Observable property for the transaction amount.
        /// </summary>
        [ObservableProperty]
        private decimal amount;

        /// <summary>
        /// Observable property for the formatted transaction date.
        /// </summary>
        [ObservableProperty]
        private string date;

        /// <summary>
        /// Observable property for the transaction notes.
        /// </summary>
        [ObservableProperty]
        private string notes;

        /// <summary>
        /// Observable property for the transaction type (Income or Expense).
        /// </summary>
        [ObservableProperty]
        private string transactionType;

        /// <summary>
        /// Initializes a new instance of the DetailViewModel with a DataService dependency.
        /// </summary>
        /// <param name="dataService">The service for database operations.</param>
        public DetailViewModel(DataService dataService)
        {
            // Assign the data service (no null check, potential oversight)
            _dataService = dataService;
            // Load transaction data (may be premature if TransactionId is not yet set)
            LoadTransactionData();
        }

        /// <summary>
        /// Handles changes to the TransactionId property by updating the value and reloading transaction data.
        /// </summary>
        /// <param name="value">The new TransactionId value.</param>
        partial void OnTransactionIdChanged(string value)
        {
            // Update the transaction ID
            _transactionId = value;
            // Reload transaction data based on the new ID
            LoadTransactionData();
        }

        /// <summary>
        /// Loads transaction data based on the current TransactionId.
        /// Retrieves transactions from the DataService, populates observable properties, and handles cases where
        /// the transaction is not found.
        /// </summary>
        private async void LoadTransactionData()
        {
            // Check if TransactionId is valid and can be parsed as a Guid
            if (!string.IsNullOrEmpty(TransactionId) && Guid.TryParse(TransactionId, out var id))
            {
                // Retrieve all transactions from the database
                var transactions = await _dataService.GetTransactionsAsync();
                // Find the transaction matching the ID
                var transaction = transactions.FirstOrDefault(t => t.Id == id);
                if (transaction != null)
                {
                    // Populate observable properties with transaction data
                    Category = transaction.Category;
                    Amount = transaction.Amount;
                    Date = transaction.Date.ToString("MMM dd, yyyy");
                    Notes = transaction.Notes ?? "None";
                    TransactionType = transaction.IsIncome ? "Income" : "Expense";
                }
                else
                {
                    // Set default values if transaction is not found
                    Category = "Not Found";
                    Amount = 0;
                    Date = "N/A";
                    Notes = "N/A";
                    TransactionType = "N/A";
                }
            }
        }

        /// <summary>
        /// Asynchronously handles the "Go Back" action by attempting to navigate up the stack or falling back to MainPage.
        /// Logs the navigation stack and any errors for debugging purposes.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [RelayCommand]
        private async Task GoBack()
        {
            // Log command execution
            System.Diagnostics.Debug.WriteLine("GoBack command executed");
            try
            {
                // Get the current navigation stack
                var currentNavigationStack = Shell.Current.Navigation.NavigationStack;
                // Log the stack count
                System.Diagnostics.Debug.WriteLine($"Current navigation stack count: {currentNavigationStack.Count}");
                // Log each page in the stack
                foreach (var page in currentNavigationStack)
                {
                    System.Diagnostics.Debug.WriteLine($"Stack page: {page.GetType().Name}");
                }
                // Attempt to navigate to the previous page
                await Shell.Current.GoToAsync("..");
                // Log successful navigation
                System.Diagnostics.Debug.WriteLine("Navigation to parent successful");
            }
            catch (Exception ex)
            {
                // Log navigation failure
                System.Diagnostics.Debug.WriteLine($"Navigation failed: {ex.Message}");
                // Fallback to navigating to MainPage
                await Shell.Current.GoToAsync("//MainPage");
                // Log fallback navigation
                System.Diagnostics.Debug.WriteLine("Fell back to MainPage");
            }
        }
    }
}