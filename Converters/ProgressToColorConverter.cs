using System.Globalization;

/// <summary>
/// A value converter that transforms a budget progress percentage into a corresponding color.
/// </summary>
namespace PersonalFinanceTracker.Converters
{
    /// <summary>
    /// Converts a progress percentage (decimal) to a color based on budget thresholds.
    /// </summary>
    public class ProgressToColorConverter : IValueConverter
    {
        /// <summary>
        /// Returns Green for progress < 80%, Yellow for 80-100%, and Red for > 100%.
        /// </summary>
        /// <param name="value">The progress percentage as a decimal value.</param>
        /// <param name="targetType">The type of the binding target property (expected to be Color).</param>
        /// <param name="parameter">Optional parameter (not used in this implementation).</param>
        /// <param name="culture">The culture to use in the conversion (not used in this implementation).</param>
        /// <returns>A Color object based on the progress threshold (Green, Yellow, or Red).</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if the input value is a decimal (progress percentage)
            if (value is decimal progress)
            {
                // Return red if progress is 100% or higher (over budget)
                if (progress >= 100) return Colors.Red;
                // Return yellow if progress is between 80% and 99.99% (nearing budget limit)
                if (progress >= 80) return Colors.Yellow;
                // Return green if progress is below 80% (within budget)
                return Colors.Green;
            }
            // Return green as a default for invalid input values
            return Colors.Green;
        }

        /// <summary>
        /// Throws a NotImplementedException if called.
        /// </summary>
        /// <param name="value">The value to convert back (not used).</param>
        /// <param name="targetType">The type to convert back to (not used).</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="culture">The culture to use in the conversion (not used).</param>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Always thrown, as ConvertBack is not supported.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Throw an exception as two-way binding is not supported
            throw new NotImplementedException();
        }
    }
}