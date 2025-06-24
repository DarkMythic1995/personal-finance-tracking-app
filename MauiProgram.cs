using Microsoft.Extensions.Logging;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker.ViewModels;
using PersonalFinanceTracker.Views;
using SkiaSharp.Views.Maui.Controls;

namespace PersonalFinanceTracker
{
    public static class MauiProgram
    {
        /// <summary>
        /// Configures and builds the MAUI application, registering services, view models, pages, and SkiaSharp handlers.
        /// </summary>
        /// <returns>A configured and built MauiApp instance.</returns>
        public static MauiApp CreateMauiApp()
        {
            // Create a MAUI app builder
            var builder = MauiApp.CreateBuilder();
            // Configure the app to use the App class as the entry point
            builder
                .UseMauiApp<App>()
                // Configure custom fonts for the application
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register SkiaSharp handler for SKCanvasView (used in ReportsPage)
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<SKCanvasView, SkiaSharp.Views.Maui.Handlers.SKCanvasViewHandler>();
            });

            // Register services with the DI container
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current); // Current connectivity service
            builder.Services.AddSingleton<DataService>(); // Data service for database operations
            builder.Services.AddSingleton<IApiService, ApiService>(); // API service for exchange rates
            builder.Services.AddSingleton<HttpClient>(); // HTTP client for API requests

            // Register view models with appropriate lifetimes
            builder.Services.AddSingleton<MainViewModel>(); // Singleton for shared main view model
            builder.Services.AddTransient<AddTransactionViewModel>(); // Transient for page-specific view model
            builder.Services.AddTransient<AddBudgetViewModel>();
            builder.Services.AddTransient<ReportsViewModel>();
            builder.Services.AddTransient<DetailViewModel>();
            builder.Services.AddTransient<EditTransactionViewModel>();

            // Register pages with appropriate lifetimes
            builder.Services.AddSingleton<MainPage>(); // Singleton for main page
            builder.Services.AddTransient<AddTransactionPage>(); // Transient for page-specific page
            builder.Services.AddTransient<AddBudgetPage>();
            builder.Services.AddTransient<ReportsPage>();
            builder.Services.AddTransient<DetailPage>();
            builder.Services.AddTransient<EditTransactionPage>();

            // Register IServiceProvider (potentially unnecessary unless used explicitly)
            builder.Services.AddSingleton(provider => provider);

#if DEBUG
            // Enable debug logging in debug builds
            builder.Logging.AddDebug();
#endif

            // Build and return the MAUI app
            return builder.Build();
        }
    }
}