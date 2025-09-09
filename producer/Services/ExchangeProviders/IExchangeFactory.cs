using producer.Models;

/// <summary>
/// This code defines a set of interfaces and a concrete implementation for fetching exchange rates from various providers.
/// The design follows the Factory and Builder patterns to promote modularity, separation of concerns, and flexibility.
/// Each interface represents a distinct part of the exchange rate retrieval process, allowing for easy extension and modification.
/// The HttpExchangeClient class encapsulates the entire process, making it straightforward to use and maintain.
/// The use of async/await ensures that the operations are non-blocking, which is essential for I/O-bound tasks like HTTP requests.
/// The overall architecture is well-suited for scenarios where multiple exchange rate providers need to be supported, each with its own URL structure, request format, and response handling.
/// The design also facilitates testing and mocking, as each component can be independently tested.
/// The use of dependency injection (e.g., passing HttpClient and builders into the HttpExchangeClient constructor) further enhances testability and flexibility.
/// The code is organized under the namespace `producer.Services.ExchangeProviders`, indicating its role within a larger application focused on currency exchange rates.
/// The comments provide clear explanations of the purpose and functionality of each interface and class, making it easier for other developers to understand and work with the code.
/// The overall design is clean, maintainable, and adheres to solid software engineering principles.
/// Abstract Factory Design Pattern: This pattern is used to create families of related or dependent objects without specifying their concrete classes.
/// In this case, the factory interface (IExchangeFactory) provides methods to create different components (URL builder, request builder, response builder) that are used to interact with exchange rate services.
/// Builder Design Pattern: Each of the builder interfaces (IUrlBuilder, IRequestBuilder, IResponseBuilder) encapsulates the construction logic for a specific part of the exchange rate retrieval process.
/// This allows for more complex construction processes to be abstracted away from the client code, promoting separation of concerns.
/// </summary>
namespace producer.Services.ExchangeProviders
{
    /// <summary>
    /// Factory interface for creating exchange rate service components.
    /// each method returns a specific builder for URL, request, or response handling.
    /// each builder is responsible for a distinct part of the exchange rate retrieval process.
    /// This design promotes separation of concerns and makes it easier to extend or modify individual components without affecting the others.
    /// </summary>
    public interface IExchangeFactory
    {
        IUrlBuilder UrlBuilder();
        IRequestBuilder RequestBuilder();
        IResponseBuilder ResponseBuilder();
    }

    /// <summary>
    /// Builder interface for constructing URLs for exchange rate requests.
    /// Each implementation will provide a method to generate a URL based on the provided parameters.
    /// This allows for flexibility in handling different exchange rate service providers, each of which may have its own URL structure and required parameters.
    /// each implementation can encapsulate the logic needed to format the URL correctly for a specific provider.
    /// This design promotes modularity and makes it easier to switch between different providers or update URL formats without affecting other parts of the system.
    /// </summary>
    public interface IUrlBuilder
    {
        string ProvideURL(string key, string from, string to);
    }

    /// <summary>
    /// Builder interface for handling HTTP requests to fetch exchange rate data.
    /// Each implementation will provide a method to send an HTTP request and return the response as a string.
    /// This allows for flexibility in handling different HTTP client configurations and request formats.
    /// </summary>
    public interface IRequestBuilder
    {
        Task<string> ProvideRequest(HttpClient client, string url, string? key = null);
    }

    /// <summary>
    /// Builder interface for processing the response from exchange rate requests.
    /// Each implementation will provide a method to deserialize the JSON response and extract the relevant exchange rate information.
    /// This allows for flexibility in handling different response formats and structures.
    /// </summary>
    public interface IResponseBuilder
    {
        Task<ExchangeRate> ProvideResponse(string json, string from, string to);
    }

    /// <summary>
    /// Client interface for fetching exchange rates using the provided builders.
    /// </summary>
    public interface IExchangeClient
    {
        Task<ExchangeRate> GetRatesAsync(string key, string from, string to, CancellationToken ct = default);
    }

    /// <summary>
    /// Concrete implementation of the IExchangeClient interface.
    /// This class uses the provided builders to construct the URL, send the HTTP request,
    /// and process the response to fetch exchange rates.
    /// It encapsulates the entire process of retrieving exchange rates, making it easy to use and maintain.
    /// </summary>
    public sealed class HttpExchangeClient : IExchangeClient
    {
        private readonly HttpClient client;
        private readonly IUrlBuilder urlBuilder;
        private readonly IRequestBuilder requestBuilder;
        private readonly IResponseBuilder responseBuilder;

        public HttpExchangeClient(HttpClient _client, IUrlBuilder _urlBuilder, IRequestBuilder _requestBuilder, IResponseBuilder _responseBuilder)
        {
            client = _client;
            urlBuilder = _urlBuilder;
            requestBuilder = _requestBuilder;
            responseBuilder = _responseBuilder;
        }

        /// <summary>
        /// Fetches exchange rates asynchronously using the provided key, from currency, and to currency.
        /// It constructs the URL, sends the HTTP request, and processes the response to return the exchange rate.
        /// If any step fails, it throws an InvalidOperationException with a relevant message.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<ExchangeRate> GetRatesAsync(string key, string from, string to, CancellationToken ct = default)
        {
            try
            {
                // Construct the URL using the URL builder
                string url = urlBuilder.ProvideURL(key, from, to);
                // Send the HTTP request and get the response as a string
                string json = await requestBuilder.ProvideRequest(client, url);
                // Process the response and extract the exchange rate
                var result = await responseBuilder.ProvideResponse(json, from, to);
                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to fetch exchange rate ({from}-{to})", ex);
            }
        }
    }
}