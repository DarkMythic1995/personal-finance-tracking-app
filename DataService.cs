using SQLite;
using PersonalFinanceTracker.Models;
using System.Diagnostics;

namespace PersonalFinanceTracker.Services
{
    /// <summary>
    /// Service class for managing data persistence using SQLite, handling CRUD operations for transactions and budgets.
    /// </summary>
    public class DataService
    {
        /// <summary>
        /// SQLite connection for asynchronous database operations.
        /// </summary>
        private readonly SQLiteAsyncConnection _database;

        /// <summary>
        /// Service for checking network connectivity.
        /// </summary>
        private readonly IConnectivity _connectivity;

        /// <summary>
        /// Initializes a new instance of the DataService, setting up the SQLite database connection.
        /// </summary>
        /// <param name="connectivity">Service for network connectivity checks.</param>
        public DataService(IConnectivity connectivity)
        {
            // Store the connectivity service
            _connectivity = connectivity;
            // Construct the database file path in the app's data directory
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "finance.db");
            // Initialize the SQLite async connection
            _database = new SQLiteAsyncConnection(dbPath);
            // Initialize the database tables asynchronously
            _ = InitializeDatabaseAsync();
        }

        /// <summary>
        /// Initializes the database by creating tables for Transaction and Budget entities.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeDatabaseAsync()
        {
            // Log initialization start
            Debug.WriteLine("DataService: Initializing database");
            // Create table for Transaction if it doesn't exist
            await _database.CreateTableAsync<PersonalFinanceTracker.Models.Transaction>();
            // Create table for Budget if it doesn't exist
            await _database.CreateTableAsync<Budget>();
            // Log initialization completion
            Debug.WriteLine("DataService: Database initialized");
        }

        /// <summary>
        /// Retrieves all transactions from the database.
        /// </summary>
        /// <returns>A list of transactions.</returns>
        public async Task<List<PersonalFinanceTracker.Models.Transaction>> GetTransactionsAsync()
        {
            // Query all transactions from the Transaction table
            var transactions = await _database.Table<PersonalFinanceTracker.Models.Transaction>().ToListAsync();
            // Log the number of retrieved transactions
            Debug.WriteLine($"DataService: Retrieved {transactions.Count} transactions");
            // Return the transaction list
            return transactions;
        }

        /// <summary>
        /// Adds a new transaction to the database, requiring an internet connection.
        /// </summary>
        /// <param name="transaction">The transaction to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddTransactionAsync(PersonalFinanceTracker.Models.Transaction transaction)
        {
            // Check for internet connectivity
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // Display alert if no internet connection
                await Application.Current.MainPage.DisplayAlert("No Internet", "You need an internet connection to add transactions.", "OK");
                return;
            }
            // Insert the transaction into the database
            await _database.InsertAsync(transaction);
            // Log the inserted transaction details
            Debug.WriteLine($"DataService: Inserted transaction: {transaction.Id}, {transaction.Category}, Notes: {transaction.Notes ?? "None"}");
        }

        /// <summary>
        /// Updates an existing transaction in the database, requiring an internet connection.
        /// </summary>
        /// <param name="transaction">The transaction to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateTransactionAsync(PersonalFinanceTracker.Models.Transaction transaction)
        {
            // Check for internet connectivity
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // Display alert if no internet connection
                await Application.Current.MainPage.DisplayAlert("No Internet", "You need an internet connection to update transactions.", "OK");
                return;
            }
            // Update the transaction in the database
            await _database.UpdateAsync(transaction);
            // Log the updated transaction details
            Debug.WriteLine($"DataService: Updated transaction: {transaction.Id}, {transaction.Category}, Amount: {transaction.Amount}");
        }

        /// <summary>
        /// Deletes a transaction from the database by ID, requiring an internet connection.
        /// </summary>
        /// <param name="id">The ID of the transaction to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteTransactionAsync(Guid id)
        {
            // Check for internet connectivity
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // Display alert if no internet connection
                await Application.Current.MainPage.DisplayAlert("No Internet", "You need an internet connection to delete transactions.", "OK");
                return;
            }
            // Delete the transaction with the specified ID
            await _database.DeleteAsync<PersonalFinanceTracker.Models.Transaction>(id);
            // Log the deleted transaction ID
            Debug.WriteLine($"DataService: Deleted transaction: {id}");
        }

        /// <summary>
        /// Retrieves all budgets from the database.
        /// </summary>
        /// <returns>A list of budgets.</returns>
        public async Task<List<Budget>> GetBudgetsAsync()
        {
            // Query all budgets from the Budget table
            var budgets = await _database.Table<Budget>().ToListAsync();
            // Log the number of retrieved budgets
            Debug.WriteLine($"DataService: Retrieved {budgets.Count} budgets");
            // Return the budget list
            return budgets;
        }

        /// <summary>
        /// Adds a new budget to the database, requiring an internet connection.
        /// </summary>
        /// <param name="budget">The budget to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddBudgetAsync(Budget budget)
        {
            // Check for internet connectivity
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // Display alert if no internet connection
                await Application.Current.MainPage.DisplayAlert("No Internet", "You need an internet connection to add budgets.", "OK");
                return;
            }
            // Insert the budget into the database
            await _database.InsertAsync(budget);
            // Log the inserted budget details
            Debug.WriteLine($"DataService: Inserted budget: {budget.Id}, {budget.Category}");
        }

        /// <summary>
        /// Deletes a budget from the database by ID, requiring an internet connection.
        /// </summary>
        /// <param name="id">The ID of the budget to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteBudgetAsync(Guid id)
        {
            // Check for internet connectivity
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // Display alert if no internet connection
                await Application.Current.MainPage.DisplayAlert("No Internet", "You need an internet connection to delete budgets.", "OK");
                return;
            }
            // Delete the budget with the specified ID
            await _database.DeleteAsync<Budget>(id);
            // Log the deleted budget ID
            Debug.WriteLine($"DataService: Deleted budget: {id}");
        }

        /// <summary>
        /// Calculates total spending for a specific category in a given month, excluding income transactions.
        /// </summary>
        /// <param name="category">The transaction category to query.</param>
        /// <param name="month">The month to filter transactions by.</param>
        /// <returns>The total spending amount.</returns>
        public async Task<decimal> GetSpendingForCategoryAsync(string category, DateTime month)
        {
            // Query transactions that are expenses (not income), match the category, and fall within the specified month
            var transactions = await _database.Table<PersonalFinanceTracker.Models.Transaction>()
                .Where(t => !t.IsIncome && t.Category == category &&
                            t.Date.Year == month.Year && t.Date.Month == month.Month)
                .ToListAsync();
            // Calculate the sum of transaction amounts
            var total = transactions.Sum(t => t.Amount);
            // Log the calculated spending
            Debug.WriteLine($"DataService: Spending for {category} in {month:yyyy-MM}: {total}");
            // Return the total spending
            return total;
        }
    }
}