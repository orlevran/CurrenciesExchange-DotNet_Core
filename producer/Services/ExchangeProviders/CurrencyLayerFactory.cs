using producer.Models;

using System.Text.Json;

namespace producer.Services.ExchangeProviders
{

    /// <summary>
    /// Concrete implementation of the IExchangeFactory for the CurrencyLayer exchange rate service.
    /// This factory provides specific builders for constructing URLs, handling HTTP requests, and processing responses from the CurrencyLayer API.
    /// Each builder encapsulates the logic needed to interact with the CurrencyLayer service, promoting modularity and separation of concerns.
    /// Classes are sealed to prevent inheritance, ensuring that the factory's behavior remains consistent and unaltered.
    /// </summary>
    public sealed class CurrencyLayerExchangeFactory : IExchangeFactory
    {
        // API key for authenticating requests to the CurrencyLayer service.
        private readonly string _key;
        // Constructor to initialize the factory with the provided API key.
        public CurrencyLayerExchangeFactory(string key) => _key = key;
        // Method to create and return a URL builder specific to CurrencyLayer.
        public IUrlBuilder UrlBuilder() => new CurrencyLayer_UrlBuilder();
        // Method to create and return a request builder specific to CurrencyLayer.
        public IRequestBuilder RequestBuilder() => new CurrencyLayer_RequestBuilder();
        // Method to create and return a response builder specific to CurrencyLayer.
        public IResponseBuilder ResponseBuilder() => new CurrencyLayer_ResponseBuilder();
    }

    /// <summary>
    /// Concrete implementation of the IUrlBuilder interface for the CurrencyLayer exchange rate service.
    /// This class constructs the appropriate URL for making requests to the CurrencyLayer API, incorporating the necessary parameters such as the API key, source currency, and target currency.
    /// It ensures that the URL is correctly formatted and encoded, allowing for successful interaction with the CurrencyLayer service.
    /// The class is sealed to prevent inheritance, ensuring that the URL construction logic remains consistent and unaltered.
    /// </summary>
    public sealed class CurrencyLayer_UrlBuilder : IUrlBuilder
    {
        public string ProvideURL(string key, string from, string to)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return $"https://api.currencylayer.com/live?access_key={Uri.EscapeDataString(key)}&currencies={to}&source={from}&format=1";
        }
    }

    /// <summary>
    /// Concrete implementation of the IRequestBuilder interface for the CurrencyLayer exchange rate service.
    /// This class builds and executes HTTP requests to the CurrencyLayer API.
    /// It uses HttpClient to send GET requests to the constructed URL and returns the response body as a string.
    /// Handles HTTP errors by logging status and response content, returning an empty string on failure.
    /// </summary>
    public sealed class CurrencyLayer_RequestBuilder : IRequestBuilder
    {
        public async Task<string> ProvideRequest(HttpClient client, string url, string? key = null)
        {
            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();

            // If HTTP error (e.g., 404), print and stop
            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}");
                Console.WriteLine(body);
                return string.Empty;
            }

            return body;
        }
    }
    
    /// <summary>
    /// Concrete implementation of the IResponseBuilder interface for the CurrencyLayer exchange rate service.
    /// This class processes the JSON response from the CurrencyLayer API, extracting the exchange rate for the specified currency pair.
    /// It deserializes the response, retrieves the relevant rate, and returns an ExchangeRate object.
    /// Handles parsing errors by throwing an InvalidOperationException with details.
    /// </summary>
    public sealed class CurrencyLayer_ResponseBuilder : IResponseBuilder
    {
        public Task<ExchangeRate> ProvideResponse(string json, string from, string to)
        {
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
                var element = (JsonElement)dict["quotes"];
                decimal rate = element.GetProperty($"{from}{to}").GetDecimal();
                return Task.FromResult(new ExchangeRate(from, to, rate));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("currencylayer: failed to parse response.", ex);
            }
        }
    }
}