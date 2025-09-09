using producer.Models;

using System.Net.Http.Headers;
using System.Text.Json;

namespace producer.Services.ExchangeProviders
{
    /// <summary>
    /// Factory and builder classes for interacting with the FXRatesAPI service.
    /// Provides URL construction, HTTP request handling, and response parsing for currency exchange rates.
    /// </summary>
    public sealed class FXRateAPIFactory : IExchangeFactory
    {
        private readonly string _key;
        public FXRateAPIFactory(string key) => _key = key;

        public IUrlBuilder UrlBuilder() => new FXR_UrlBuilder();
        public IRequestBuilder RequestBuilder() => new FXR_RequestBuilder();
        public IResponseBuilder ResponseBuilder() => new FXR_ResponseBuilder();
    }

    public sealed class FXR_UrlBuilder : IUrlBuilder
    {
        public string ProvideURL(string key, string from, string to)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return $"https://api.fxratesapi.com/latest?base={from}&symbols={to}";
        }
    }

    public sealed class FXR_RequestBuilder : IRequestBuilder
    {
        public async Task<string> ProvideRequest(HttpClient client, string url, string? key = null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}");
                Console.WriteLine(body); // shows any API error payload
                return string.Empty;
            }

            return body;
        }
    }

    public sealed class FXR_ResponseBuilder : IResponseBuilder
    {
        public Task<ExchangeRate> ProvideResponse(string json, string from, string to)
        {
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
                var element = (JsonElement)dict["rates"];
                decimal rate = element.GetProperty(to).GetDecimal();
                return Task.FromResult(new ExchangeRate(from, to, rate));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("fxratesapi: failed to parse response.", ex);
            }
        }
    }
}