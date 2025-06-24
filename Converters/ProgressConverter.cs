using PersonalFinanceTracker.ViewModels;
using System.Diagnostics;
using System.Globalization;

/// <summary>
/// A value converter that transforms a budget category string into a progress value (0-1) for use in a ProgressBar.
/// </summary>
namespace PersonalFinanceTracker.Converters
{
    /// <summary>
    /// Converts a category string to a progress value based on budget data from the MainViewModel.
    /// </summary>
    public class ProgressConverter : IValueConverter
    {
        /// <summary>
        /// Converts a category string to a progress value (0-1) by querying the MainViewModel.
        /// </summary>
        /// <param name="value">The category string representing the budget category.</param>
        /// <param name="targetType">The type of the binding target property (e.g., double for ProgressBar).</param>
        /// <param name="parameter">The MainViewModel instance used to calculate progress.</param>
        /// <param name="culture">The culture to use in the conversion (not used).</param>
        /// <returns>A decimal value between 0 and 1 representing the progress, or 0 on error.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if the input value is a string (the budget category)
            if (value is string category)
            {
                // Log the category being processed
                Debug.WriteLine($"ProgressConverter: Category = {category}");
                // Check if the parameter is a MainViewModel instance
                if (parameter is MainViewModel vm)
                {
                    // Log that MainViewModel was found
                    Debug.WriteLine($"ProgressConverter: Found MainViewModel, calculating progress for {category}");
                    try
                    {
                        // Run the async GetBudgetProgressAsync method to get the progress percentage
                        var progressTask = Task.Run(() => vm.GetBudgetProgressAsync(category));
                        // Block to get the result (temporary, noted to switch to Behavior)
                        var progress = progressTask.Result; // Blocking for now, will switch to Behavior
                        // Log the calculated progress
                        Debug.WriteLine($"ProgressConverter: Progress for {category} = {progress}%");
                        // Convert the percentage (0-100) to a 0-1 range for ProgressBar
                        return progress / 100; // Convert to 0-1 range for ProgressBar
                    }
                    catch (Exception ex)
                    {
                        // Log any errors during progress calculation
                        Debug.WriteLine($"ProgressConverter: Error calculating progress - {ex.Message}");
                        // Return 0 if an error occurs
                        return 0m;
                    }
                }
                else
                {
                    // Log if MainViewModel was not provided
                    Debug.WriteLine("ProgressConverter: MainViewModel not passed via parameter");
                    // Return 0 if MainViewModel is missing
                    return 0m;
                }
            }
            // Log if the input value is not a string
            Debug.WriteLine("ProgressConverter: Invalid value type");
            // Return 0 if the input value is invalid
            return 0m;
        }

        /// <summary>
        /// Not implemented, as this converter is intended for one-way binding only.
        /// Throws a NotImplementedException if called.
        /// </summary>
        /// <param name="value">The value to convert back.</param>
        /// <param name="targetType">The type to convert back to.</param>
        /// <param name="parameter">Optional parameter.</param>
        /// <param name="culture">The culture to use in the conversion.</param>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Always thrown, as ConvertBack is not supported.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Throw an exception as two-way binding is not supported
            throw new NotImplementedException();
        }
    }
}