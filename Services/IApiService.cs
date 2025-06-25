using System.Text.Json;
using System.Text.Json.Serialization;

namespace PersonalFinanceTracker.Services
{
    /// <summary>
    /// Interface defining the contract for fetching exchange rates.
    /// </summary>
    public interface IApiService
    {
        /// <summary>
        /// Retrieves the exchange rate between two currencies.
        /// </summary>
        /// <param name="fromCurrency">The source currency (e.g., USD).</param>
        /// <param name="toCurrency">The target currency (e.g., JPY).</param>
        /// <returns>The exchange rate as a decimal.</returns>
        Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
    }

    /// <summary>
    /// Service for fetching exchange rates from the Alpha Vantage API.
    /// </summary>
    public class ApiService : IApiService
    {
        // HttpClient instance for making API requests
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of ApiService with an HttpClient.
        /// </summary>
        /// <param name="httpClient">The HttpClient for making API requests.</param>
        public ApiService(HttpClient httpClient)
        {
            // Validate and assign the HttpClient
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            // Set the base address for the Alpha Vantage API
            _httpClient.BaseAddress = new Uri("https://alpha-vantage.p.rapidapi.com/");
            // Add required API headers
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "alpha-vantage.p.rapidapi.com");
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", "7c08ca6013mshe0316f46435dca1p1e9671jsn3c9bf7325824");
        }

        /// <summary>
        /// Fetches the exchange rate between two currencies from the Alpha Vantage API.
        /// </summary>
        /// <param name="fromCurrency">The source currency (e.g., USD).</param>
        /// <param name="toCurrency">The target currency (e.g., JPY).</param>
        /// <returns>The exchange rate as a decimal.</returns>
        /// <exception cref="HttpRequestException">Thrown if the API call fails.</exception>
        /// <exception cref="Exception">Thrown for API errors or invalid data.</exception>
        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            try
            {
                // Construct the HTTP GET request with query parameters
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"query?function=CURRENCY_EXCHANGE_RATE&from_currency={fromCurrency}&to_currency={toCurrency}", UriKind.Relative)
                };

                // Send the request and get the response
                var response = await _httpClient.SendAsync(request);
                // Read the response content as a string
                var json = await response.Content.ReadAsStringAsync();
                // Log the raw API response for debugging
                System.Diagnostics.Debug.WriteLine($"API Response: {json}");

                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    // Throw an exception with the status code and response content
                    throw new HttpRequestException($"API call failed with status {response.StatusCode}: {json}");
                }

                // Configure JSON deserialization options to ignore case
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                // Deserialize the JSON into an ExchangeRateResponse object
                var data = JsonSerializer.Deserialize<ExchangeRateResponse>(json, options);

                // Check for API error messages
                if (data?.ErrorMessage != null)
                {
                    // Throw an exception with the API error message
                    throw new Exception($"API Error: {data.ErrorMessage}");
                }

                // Validate and parse the exchange rate
                if (data?.RealtimeCurrencyExchangeRate?.ExchangeRate != null &&
                    decimal.TryParse(data.RealtimeCurrencyExchangeRate.ExchangeRate, out var rate))
                {
                    // Log the parsed exchange rate
                    System.Diagnostics.Debug.WriteLine($"Parsed Exchange Rate: {rate}");
                    // Return the parsed rate
                    return rate;
                }

                // Throw an exception if the exchange rate data is invalid
                throw new Exception("Invalid exchange rate data received.");
            }
            catch (Exception ex)
            {
                // Log the error details
                System.Diagnostics.Debug.WriteLine($"GetExchangeRateAsync error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                // Re-throw the exception for upstream handling
                throw;
            }
        }
    }

    /// <summary>
    /// Represents the JSON response from the Alpha Vantage API.
    /// </summary>
    internal class ExchangeRateResponse
    {
        // Nested object containing the exchange rate data
        [JsonPropertyName("Realtime Currency Exchange Rate")]
        public RealtimeCurrencyExchangeRate RealtimeCurrencyExchangeRate { get; set; }

        // Error message from the API, if any
        [JsonPropertyName("Error Message")]
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Represents the exchange rate data within the API response.
    /// </summary>
    internal class RealtimeCurrencyExchangeRate
    {
        // The exchange rate as a string
        [JsonPropertyName("5. Exchange Rate")]
        public string ExchangeRate { get; set; }
    }
}