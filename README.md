# Personal Finance Tracker

## Overview
I created this Personal Finance Tracking application for my Intro to Mobile class at Mid-State Technical College (MSTC) this summer. It’s a .NET MAUI application designed to help users manage their finances by tracking transactions and budgets. The goal is to provide a user-friendly interface to add transactions and budgets, view detailed reports with custom charts, and navigate between pages seamlessly. This update includes enhancements based on instructor feedback, new features developed over the weeks, and the integration of a currency exchange rate API via RapidAPI.

## Features
- Add and manage transactions (income and expenses) with categories and notes, now with improved accessibility and validation feedback.
- Set and track monthly budgets for various categories, with a stable "Recent Transactions" label alignment.
- View detailed transaction information on the DetailPage.
- Generate reports with bar charts for category spending and line charts for monthly trends.
- Display real-time currency exchange rates using the Alpha Vantage API, integrated via RapidAPI.
- Responsive design across Android, iOS, macCatalyst, and Windows platforms, with enhanced button layouts.
- Theme (light/dark) toggle with updated background images for a more financial app-like aesthetic.

## Usage
- Navigate to the MainPage to view transactions, budgets, and the latest currency exchange rate (e.g., "1 USD = 145.90 JPY").
- Use Add Transaction and Add Budget pages to input new entries, featuring wider "Save" and "Cancel" buttons with increased spacing.
- Check ReportsPage for visual insights into your finances.
- View transaction details on the DetailPage and edit them with improved validation.

## API Integration
This application integrates the Alpha Vantage API, accessed through RapidAPI, to fetch real-time currency exchange rates. The API provides data such as the exchange rate between USD and JPY, which is displayed on the MainPage. Key details include:
- **API Endpoint**: The CURRENCY_EXCHANGE_RATE function from Alpha Vantage.
- **Authentication**: A secure API key is stored and used to authenticate requests, ensuring only authorized access to the data.
- **Implementation**: The `ApiService` class handles HTTP requests using `HttpClient`, parsing the JSON response to extract the exchange rate, which is then bound to the UI via the `MainViewModel`.
- **Usage**: The app fetches the latest rate on app startup or refresh, providing users with up-to-date financial information.

## Changes from Week 1
### Instructor Feedback Addressed
- **Switch Labeling for Accessibility**: Improved the Expense/Income switch with `AutomationProperties.Name` for screen readers, clarifying its function ("Toggle Income/Expense: Off for Expense, On for Income"). Added a visible description label for better user understanding.
- **Enhance Button Layout and Spacing**: Wrapped "Save" and "Cancel" buttons in a `HorizontalStackLayout` with increased `Spacing` (20) and `Padding` (15), and added `WidthRequest` (120) to buttons on the Add Transaction page for a clearer separation and consistent appearance.
- **Consistent Input Validation Feedback**: Added validation messages near inputs on the Add Transaction and Edit Transaction pages (e.g., under Entry for amount), using a Label bound to `ValidationMessage` to display errors immediately. Integrated `IsValid` binding cues are planned for future updates.

### Week 2 Enhancements
- **UI Improvements**: Adjusted the "Recent Transactions" label alignment on MainPage with `Margin` and `VerticalOptions` for a static position. Increased width and spacing for buttons on `AddTransactionPage`.
- **Code Documentation**: Added comprehensive XML comments to `MainPage`, `MainViewModel`, `EditTransactionViewModel`, and `DataService` classes for better maintainability.
- **Accessibility**: Enhanced switch accessibility with descriptive labels and automation properties.
- **Theme Updates**: Replaced background images with financial app-inspired designs ("dark_background_2.png" and "light_background_1.png") for a more creative and professional look.
- **API Integration**: Added support for currency exchange rates using the Alpha Vantage API via RapidAPI, enhancing the app’s financial tracking capabilities.

## Demo
Here are links to my YouTube walkthroughs:
- [Week 1 Personal Finance Tracker Demo](https://youtu.be/LqpPsed4ooM)
- [Week 2 Personal Finance Tracker Update](https://youtu.be/ZOax53qnHuU)

## Technologies Used
- **.NET MAUI**: Cross-platform framework for building the app.
- **CommunityToolkit.Mvvm**: For MVVM pattern implementation.
- **SQLite-net-pcl**: For local database management.
- **SkiaSharp**: For custom chart rendering in reports.
- **Microsoft.Maui.Networking**: For network connectivity checks.
- **Alpha Vantage API (via RapidAPI)**: For real-time currency exchange rate data.

## License
[MIT License]

## Acknowledgments
Thanks to my instructor Brent Presley at Mid-State Technical College for valuable feedback.
Additional thanks to the xAI, Codecademy, and other resources for guidance during development.
