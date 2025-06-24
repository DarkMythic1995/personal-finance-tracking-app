using CommunityToolkit.Mvvm.ComponentModel;
using PersonalFinanceTracker.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

/// <summary>
/// ViewModel responsible for generating and managing financial reports in the Personal Finance Tracker application.
/// </summary>
namespace PersonalFinanceTracker.ViewModels
{
    /// <summary>
    /// A ViewModel class that provides the logic for displaying financial reports, including category and monthly spending.
    /// </summary>
    public partial class ReportsViewModel : ObservableObject
    {
        /// <summary>
        /// Service for database operations.
        /// </summary>
        private readonly DataService _dataService;

        /// <summary>
        /// Current month for filtering transactions.
        /// </summary>
        private readonly DateTime _currentMonth;

        /// <summary>
        /// Observable collection for category-based spending data.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<(string Category, decimal Amount)> categorySpendings;

        /// <summary>
        /// Observable collection for monthly spending data.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<(DateTime Month, decimal Amount)> monthlySpendings;

        /// <summary>
        /// Initializes a new instance of the ReportsViewModel with a DataService dependency.
        /// Sets up the data service, initializes the current month, and starts loading report data asynchronously.
        /// </summary>
        /// <param name="dataService">The service for database operations.</param>
        public ReportsViewModel(DataService dataService)
        {
            // Assign the data service.
            _dataService = dataService;
            // Set current month to the first day of the current month
            _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            // Initialize category spendings collection
            CategorySpendings = new ObservableCollection<(string Category, decimal Amount)>();
            // Initialize monthly spendings collection
            MonthlySpendings = new ObservableCollection<(DateTime Month, decimal Amount)>();
            // Log initialization
            Debug.WriteLine("ReportsViewModel: Initialized, starting LoadReportDataAsync");
            // Start loading report data
            _ = LoadReportDataAsync();
        }

        /// <summary>
        /// Asynchronously loads report data, including category and monthly spending, from the database.
        /// Filters transactions for the current month, calculates spending per category, and aggregates spending
        /// for the past six months, updating observable collections on success or logging errors.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task LoadReportDataAsync()
        {
            // Log method start
            Debug.WriteLine("ReportsViewModel: Starting LoadReportDataAsync");
            try
            {
                // Retrieve budgets from the database
                var budgets = await _dataService.GetBudgetsAsync();
                // Log number of budgets retrieved
                Debug.WriteLine($"ReportsViewModel: Retrieved {budgets.Count} budgets");
                // Retrieve transactions from the database
                var transactions = await _dataService.GetTransactionsAsync();
                // Log number of transactions retrieved
                Debug.WriteLine($"ReportsViewModel: Retrieved {transactions.Count} transactions");

                // Filter transactions for the current month (excluding income)
                var currentMonthTransactions = transactions.Where(t => t.Date.Month == _currentMonth.Month && t.Date.Year == _currentMonth.Year && !t.IsIncome).ToList();
                // Log filtered transactions
                Debug.WriteLine($"ReportsViewModel: Filtered {currentMonthTransactions.Count} transactions for current month {_currentMonth:MMM yyyy}");

                // Get distinct categories from budgets
                var categories = budgets.Select(b => b.Category).Distinct();
                // Clear existing category spendings
                CategorySpendings.Clear();
                // Calculate spending for each category
                foreach (var category in categories)
                {
                    var spent = currentMonthTransactions.Where(t => t.Category == category).Sum(t => t.Amount);
                    CategorySpendings.Add((Category: category, Amount: spent));
                    // Log category spending
                    Debug.WriteLine($"ReportsViewModel: Added category spending - {category}: {spent}");
                }

                // Calculate spending for the past six months
                var startMonth = _currentMonth.AddMonths(-5);
                MonthlySpendings.Clear();
                for (var month = startMonth; month <= _currentMonth; month = month.AddMonths(1))
                {
                    // Sum transactions for the month (excluding income)
                    var monthlySpent = transactions.Where(t => t.Date >= month && t.Date < month.AddMonths(1) && !t.IsIncome).Sum(t => t.Amount);
                    MonthlySpendings.Add((Month: month, Amount: monthlySpent));
                    // Log monthly spending
                    Debug.WriteLine($"ReportsViewModel: Added monthly spending - {month:MMM yyyy}: {monthlySpent}");
                }
            }
            catch (Exception ex)
            {
                // Log any errors
                Debug.WriteLine($"ReportsViewModel: Error in LoadReportDataAsync - {ex.Message}");
            }
            // Log method completion
            Debug.WriteLine("ReportsViewModel: LoadReportDataAsync completed");
        }

        /// <summary>
        /// Navigates back to the main page of the application.
        /// </summary>
        /// <remarks>This method uses the Shell navigation system to navigate to the route "//MainPage".
        /// Ensure that the route "//MainPage" is registered in the Shell configuration.</remarks>
        /// <returns>A task that represents the asynchronous navigation operation.</returns>
        [RelayCommand]
        private async Task GoBack()
        {
            // Navigate to MainPage using Shell navigation
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}