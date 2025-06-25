using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinanceTracker.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

/// <summary>
/// ViewModel responsible for managing the editing of transactions in the Personal Finance Tracker application.
/// </summary>
namespace PersonalFinanceTracker.ViewModels
{
    /// <summary>
    /// A ViewModel class that provides the logic for editing an existing transaction entry.
    /// Includes observable properties for data binding and relay commands for save and cancel actions.
    /// </summary>
    [QueryProperty(nameof(TransactionId), "TransactionId")]
    public partial class EditTransactionViewModel : ObservableObject
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
        /// Observable property for the transaction's unique identifier.
        /// </summary>
        [ObservableProperty]
        private string transactionId;

        /// <summary>
        /// Observable property for the transaction being edited.
        /// </summary>
        [ObservableProperty]
        private PersonalFinanceTracker.Models.Transaction selectedTransaction;

        /// <summary>
        /// Observable collection of predefined transaction categories for UI selection.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> categories = new()
        {
            "Groceries", "Transport", "Salary", "Dining Out", "Entertainment", "Utilities"
        };

        /// <summary>
        /// Observable property for displaying validation errors to the UI.
        /// </summary>
        [ObservableProperty]
        private string validationMessage;

        /// <summary>
        /// Initializes a new instance of the EditTransactionViewModel with required services.
        /// Sets up the DataService, connectivity, and MainViewModel dependencies.
        /// </summary>
        /// <param name="dataService">The service for database operations.</param>
        /// <param name="connectivity">The service for checking network connectivity.</param>
        /// <param name="mainViewModel">The main view model for refreshing data after changes.</param>
        public EditTransactionViewModel(DataService dataService, IConnectivity connectivity, MainViewModel mainViewModel)
        {
            // Validate and assign the data service
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            // Validate and assign the connectivity service
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            // Validate and assign the main view model
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            // Log initialization
            Debug.WriteLine("EditTransactionViewModel: Initialized");
        }

        /// <summary>
        /// Handles changes to the TransactionId property by loading the transaction data.
        /// </summary>
        /// <param name="value">The new TransactionId value.</param>
        partial void OnTransactionIdChanged(string value)
        {
            // Update the transaction ID
            TransactionId = value;
            // Log the change
            Debug.WriteLine($"EditTransactionViewModel: TransactionId changed to {value}");
            // Load transaction data
            LoadTransactionAsync();
        }

        /// <summary>
        /// Asynchronously loads the transaction data based on the TransactionId.
        /// </summary>
        private async void LoadTransactionAsync()
        {
            // Log entry to method
            Debug.WriteLine("EditTransactionViewModel: Entering LoadTransactionAsync");
            try
            {
                // Check if TransactionId is valid and can be parsed as a Guid
                if (!string.IsNullOrEmpty(TransactionId) && Guid.TryParse(TransactionId, out var id))
                {
                    // Retrieve all transactions from the database
                    var transactions = await _dataService.GetTransactionsAsync();
                    // Find the transaction matching the ID or create a default
                    SelectedTransaction = transactions.FirstOrDefault(t => t.Id == id) ?? new PersonalFinanceTracker.Models.Transaction
                    {
                        Id = id,
                        Date = DateTime.Today,
                        IsIncome = false
                    };
                    // Log the loaded transaction
                    Debug.WriteLine($"EditTransactionViewModel: Loaded transaction {SelectedTransaction?.Id}, Category: {SelectedTransaction?.Category}");
                }
                else
                {
                    // Log invalid or empty TransactionId
                    Debug.WriteLine("EditTransactionViewModel: Invalid or empty TransactionId");
                    // Set a default transaction
                    SelectedTransaction = new PersonalFinanceTracker.Models.Transaction { Date = DateTime.Today, IsIncome = false };
                }
            }
            catch (Exception ex)
            {
                // Log any errors
                Debug.WriteLine($"EditTransactionViewModel: Error in LoadTransactionAsync - {ex.Message}");
                // Set a default transaction as a fallback
                SelectedTransaction = new PersonalFinanceTracker.Models.Transaction { Date = DateTime.Today, IsIncome = false };
            }
        }

        /// <summary>
        /// Saves the edited transaction if validation and connectivity checks pass, then navigates back.
        /// Validates that the amount is positive and a category is selected, checks for internet access,
        /// updates the transaction, refreshes the main view model, and navigates back.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [RelayCommand]
        private async Task Save()
        {
            // Log command execution
            Debug.WriteLine("EditTransactionViewModel: Save command triggered");
            try
            {
                // Clear previous validation message
                ValidationMessage = null;
                // Check if transaction is null
                if (SelectedTransaction == null)
                {
                    ValidationMessage = "No transaction selected.";
                    // Log validation failure
                    Debug.WriteLine("EditTransactionViewModel: Save failed - SelectedTransaction is null");
                    return;
                }
                // Validate transaction amount
                if (SelectedTransaction.Amount <= 0)
                {
                    ValidationMessage = "Please enter a valid amount greater than zero.";
                    // Log validation failure
                    Debug.WriteLine("EditTransactionViewModel: Save failed - Invalid amount");
                    return;
                }
                // Validate transaction category
                if (string.IsNullOrEmpty(SelectedTransaction.Category))
                {
                    ValidationMessage = "Please select a category.";
                    // Log validation failure
                    Debug.WriteLine("EditTransactionViewModel: Save failed - No category selected");
                    return;
                }
                // Log network status
                Debug.WriteLine($"EditTransactionViewModel: NetworkAccess is {_connectivity.NetworkAccess}");
                // Check for internet connectivity
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    ValidationMessage = "No internet connection available.";
                    // Log alert display
                    Debug.WriteLine("EditTransactionViewModel: Displaying no internet alert");
                    // Display alert on the main thread
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("No Internet", "An internet connection is required to save.", "OK");
                    });
                    // Log alert completion
                    Debug.WriteLine("EditTransactionViewModel: No internet alert displayed");
                    return;
                }
                // Log transaction update attempt
                Debug.WriteLine($"EditTransactionViewModel: Updating transaction: {SelectedTransaction.Category}, {SelectedTransaction.Amount}, {SelectedTransaction.Date}, IsIncome: {SelectedTransaction.IsIncome}");
                // Update the transaction in the database
                await _dataService.UpdateTransactionAsync(SelectedTransaction);
                // Log successful update
                Debug.WriteLine("EditTransactionViewModel: Transaction updated in database");
                // Refresh the main view model's data
                await _mainViewModel.InitializeAsync();
                // Log refresh completion
                Debug.WriteLine("EditTransactionViewModel: MainViewModel refreshed");
                // Navigate back to MainPage
                await NavigateBackAsync();
            }
            catch (Exception ex)
            {
                // Set error message for UI
                ValidationMessage = "An error occurred while saving.";
                // Log error details
                Debug.WriteLine($"EditTransactionViewModel: Save error - {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Cancels the transaction edit and navigates back.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [RelayCommand]
        private async Task Cancel()
        {
            // Log command execution
            Debug.WriteLine("EditTransactionViewModel: Cancel command triggered");
            try
            {
                // Navigate back to MainPage
                await NavigateBackAsync();
            }
            catch (Exception ex)
            {
                // Log navigation error
                Debug.WriteLine($"EditTransactionViewModel: Cancel error - {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Navigates back to MainPage using Shell navigation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task NavigateBackAsync()
        {
            // Log navigation attempt
            Debug.WriteLine("EditTransactionViewModel: Attempting navigation back");
            try
            {
                // Navigate to MainPage using Shell navigation
                await Shell.Current.GoToAsync("//MainPage");
                // Log successful navigation
                Debug.WriteLine("EditTransactionViewModel: Navigation to MainPage successful");
            }
            catch (Exception ex)
            {
                // Log navigation failure
                Debug.WriteLine($"EditTransactionViewModel: Navigation failed - {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }
    }
}