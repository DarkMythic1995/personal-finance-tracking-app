using PersonalFinanceTracker.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Diagnostics;

/// <summary>
/// A ContentPage representing the user interface for displaying financial reports in the Personal Finance Tracker application.
/// </summary>
namespace PersonalFinanceTracker.Views
{
    /// <summary>
    /// A page class that provides the UI for viewing financial reports, including category and monthly spending charts.
    /// </summary>
    public partial class ReportsPage : ContentPage
    {
        /// <summary>
        /// View model for managing report data.
        /// </summary>
        private readonly ReportsViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the ReportsPage with a specified view model.
        /// Sets up the page's UI components, binds the view model, and triggers asynchronous data loading and chart invalidation.
        /// </summary>
        /// <param name="vm">The ReportsViewModel instance to bind to this page.</param>
        public ReportsPage(ReportsViewModel vm)
        {
            // Initialize XAML-defined UI components (e.g., CategoryChart and MonthlyChart)
            InitializeComponent();
            // Assign the injected view model
            _viewModel = vm;
            // Set the view model as the binding context for data binding
            BindingContext = _viewModel;
            // Start async data loading and chart invalidation
            _ = LoadDataAndInvalidateAsync();
        }

        /// <summary>
        /// Asynchronously loads report data and sets up event handling for chart invalidation.
        /// Introduces a delay to allow data to load, subscribes to property changes, and initially invalidates chart surfaces.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task LoadDataAndInvalidateAsync()
        {
            // Log the start of data loading
            Debug.WriteLine("ReportsPage: Waiting for data to load");
            // Delay to allow view model data to load (temporary workaround)
            await Task.Delay(100);
            // Subscribe to view model property changes to update charts
            _viewModel.PropertyChanged += (s, e) =>
            {
                // Check if CategorySpendings or MonthlySpendings changed
                if (e.PropertyName == nameof(ReportsViewModel.CategorySpendings) ||
                    e.PropertyName == nameof(ReportsViewModel.MonthlySpendings))
                {
                    // Invalidate chart surfaces to trigger redraw
                    CategoryChart.InvalidateSurface();
                    MonthlyChart.InvalidateSurface();
                    // Log chart invalidation
                    Debug.WriteLine($"ReportsPage: Invalidated surfaces for {e.PropertyName}");
                }
            };
            // Initially invalidate chart surfaces
            CategoryChart.InvalidateSurface();
            MonthlyChart.InvalidateSurface();
            // Log initial invalidation
            Debug.WriteLine("ReportsPage: Initial surfaces invalidated");
        }

        /// <summary>
        /// Handles the paint surface event for the category chart, rendering a bar chart of category spending.
        /// Clears the canvas, calculates bar heights based on maximum spending, and draws bars with category labels.
        /// </summary>
        /// <param name="sender">The sender of the event (typically the SKCanvasView).</param>
        /// <param name="e">The event arguments containing the surface and info for painting.</param>
        private void OnCategoryChartPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            // Log chart rendering
            Debug.WriteLine("ReportsPage: OnCategoryChartPaintSurface called");
            // Get the canvas and drawing info
            var canvas = e.Surface.Canvas;
            // Clear the canvas with a white background
            canvas.Clear(SKColors.White);
            var info = e.Info;
            var width = info.Width;
            var height = info.Height;

            // Check if there is category spending data
            if (!_viewModel.CategorySpendings.Any())
            {
                // Log empty data and exit
                Debug.WriteLine("ReportsPage: No category spendings data");
                return;
            }

            // Calculate the maximum spending amount for scaling
            var maxAmount = _viewModel.CategorySpendings.Max(cs => cs.Amount);
            // Calculate bar width based on number of categories
            var barWidth = width / (_viewModel.CategorySpendings.Count() * 2);
            // Define colors for bars
            var colors = new[] { SKColors.Blue, SKColors.Green, SKColors.Red, SKColors.Purple };

            // Create a paint object for drawing
            using var paint = new SKPaint { IsAntialias = true };
            // Iterate through category spendings
            for (int i = 0; i < _viewModel.CategorySpendings.Count(); i++)
            {
                // Get the current spending entry
                var spending = _viewModel.CategorySpendings.ElementAt(i);
                // Calculate bar height, ensuring a minimum height of 5 pixels
                var barHeight = maxAmount == 0 ? 0 : Math.Max(5, (float)(spending.Amount / maxAmount * (height - 70)));
                // Set bar color (cycle through colors array)
                paint.Color = colors[i % colors.Length];
                // Draw the bar rectangle
                canvas.DrawRect(i * barWidth * 2, height - barHeight, barWidth, barHeight, paint);
                // Set text color to black
                paint.Color = SKColors.Black;
                // Create a font for category labels
                using var font = new SKFont(SKTypeface.Default, 20);
                // Calculate x-position for centered text
                var x = i * barWidth * 2 + barWidth / 2;
                // Draw category label below the bar
                canvas.DrawText(spending.Category, x, height - 20, SKTextAlign.Center, font, paint);
            }
            // Log successful rendering
            Debug.WriteLine($"ReportsPage: Rendered CategoryChart with {_viewModel.CategorySpendings.Count()} items");
        }

        /// <summary>
        /// Handles the paint surface event for the monthly chart, rendering a line chart of monthly spending.
        /// Clears the canvas, calculates points based on maximum spending, draws lines, and adds rotated month labels.
        /// </summary>
        /// <param name="sender">The sender of the event (typically the SKCanvasView).</param>
        /// <param name="e">The event arguments containing the surface and info for painting.</param>
        private void OnMonthlyChartPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            // Log chart rendering
            Debug.WriteLine("ReportsPage: OnMonthlyChartPaintSurface called");
            // Get the canvas and drawing info
            var canvas = e.Surface.Canvas;
            // Clear the canvas with a white background
            canvas.Clear(SKColors.White);
            var info = e.Info;
            var width = info.Width;
            var height = info.Height;

            // Check if there is monthly spending data
            if (!_viewModel.MonthlySpendings.Any())
            {
                // Log empty data and exit
                Debug.WriteLine("ReportsPage: No monthly spendings data");
                return;
            }

            // Calculate the maximum spending amount for scaling
            var maxAmount = _viewModel.MonthlySpendings.Max(ms => ms.Amount);
            if (maxAmount == 0)
            {
                // Log zero amount and exit
                Debug.WriteLine("ReportsPage: Max amount is zero, no scaling possible");
                return;
            }
            // Calculate x-step for points (handle single data point case)
            var stepX = _viewModel.MonthlySpendings.Count() > 1 ? width / (_viewModel.MonthlySpendings.Count() - 1) : width;
            // Calculate points for the line chart
            var points = _viewModel.MonthlySpendings
                .Select((ms, i) => new SKPoint(i * stepX, height - (float)(ms.Amount / maxAmount * (height - 70))))
                .ToArray();

            // Create a paint object for drawing lines
            using var paint = new SKPaint { IsAntialias = true, Color = SKColors.Blue, StrokeWidth = 2 };
            // Draw lines connecting points
            for (int i = 1; i < points.Length; i++)
            {
                canvas.DrawLine(points[i - 1], points[i], paint);
            }
            // Set text color to black
            paint.Color = SKColors.Black;
            // Create a font for month labels
            using var font = new SKFont(SKTypeface.Default, 20);
            // Iterate through monthly spendings for labels
            for (int i = 0; i < _viewModel.MonthlySpendings.Count(); i++)
            {
                // Calculate x-position for the label
                var x = i * stepX;
                // Format month as three-letter abbreviation
                var monthText = _viewModel.MonthlySpendings.ElementAt(i).Month.ToString("MMM");
                // Save canvas state for rotation
                canvas.Save();
                // Rotate the canvas for angled text
                canvas.RotateDegrees(-45, x, height - 20);
                // Draw the month label
                canvas.DrawText(monthText, x, height - 20, SKTextAlign.Left, font, paint);
                // Restore the canvas state
                canvas.Restore();
            }
            // Log successful rendering
            Debug.WriteLine($"ReportsPage: Rendered MonthlyChart with {_viewModel.MonthlySpendings.Count()} items");
        }
    }
}