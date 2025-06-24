using PersonalFinanceTracker.ViewModels;

namespace PersonalFinanceTracker
{
    /// <summary>
    /// The main application class for the Personal Finance Tracker, responsible for initializing the app and creating the main window.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// View model for the main page.
        /// </summary>
        private readonly MainViewModel _mainViewModel;

        /// <summary>
        /// View model for adding transactions.
        /// </summary>
        private readonly AddTransactionViewModel _addTransactionViewModel;

        /// <summary>
        /// View model for adding budgets.
        /// </summary>
        private readonly AddBudgetViewModel _addBudgetViewModel;

        /// <summary>
        /// View model for generating reports.
        /// </summary>
        private readonly ReportsViewModel _reportsViewModel;

        /// <summary>
        /// Initializes a new instance of the App class with injected view models.
        /// </summary>
        /// <param name="mainViewModel">The view model for the main page.</param>
        /// <param name="addTransactionViewModel">The view model for adding transactions.</param>
        /// <param name="addBudgetViewModel">The view model for adding budgets.</param>
        /// <param name="reportsViewModel">The view model for generating reports.</param>
        public App(MainViewModel mainViewModel, AddTransactionViewModel addTransactionViewModel,
                   AddBudgetViewModel addBudgetViewModel, ReportsViewModel reportsViewModel)
        {
            // Initialize XAML-defined resources.
            InitializeComponent();
            // Store the injected view models
            _mainViewModel = mainViewModel;
            _addTransactionViewModel = addTransactionViewModel;
            _addBudgetViewModel = addBudgetViewModel;
            _reportsViewModel = reportsViewModel;
        }

        /// <summary>
        /// Creates the main window for the application, hosting the navigation shell.
        /// </summary>
        /// <param name="activationState">The activation state (not used).</param>
        /// <returns>A Window instance containing the app's navigation shell or an error page if initialization fails.</returns>
        protected override Window CreateWindow(IActivationState activationState)
        {
            try
            {
                // Create a new window with the AppShell, passing required view models
                return new Window(new AppShell(_mainViewModel, _addTransactionViewModel, _addBudgetViewModel));
            }
            catch (Exception ex)
            {
                // Log the error details for debugging
                System.Diagnostics.Debug.WriteLine($"Window Creation Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                // Return a fallback window with an error message
                return new Window(new ContentPage
                {
                    Content = new Label
                    {
                        // Display the error message in the UI
                        Text = $"Error: {ex.Message}",
                        // Center the label horizontally
                        HorizontalOptions = LayoutOptions.Center,
                        // Center the label vertically
                        VerticalOptions = LayoutOptions.Center
                    }
                });
            }
        }
    }
}