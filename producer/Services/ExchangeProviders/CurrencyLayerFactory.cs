using producer.Models;

using System.Text.Json;

namespace producer.Services.ExchangeProviders
{

    public sealed class CurrencyLayerExchangeFactory : IExchangeFactory
    {
        private readonly string _key;
        public CurrencyLayerExchangeFactory(string key) => _key = key;

        public IUrlBuilder UrlBuilder() => new CurrencyLayer_UrlBuilder();
        public IRequestBuilder RequestBuilder() => new CurrencyLayer_RequestBuilder();
        public IResponseBuilder ResponseBuilder() => new CurrencyLayer_ResponseBuilder();
    }

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