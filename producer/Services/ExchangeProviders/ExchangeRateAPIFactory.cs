using producer.Models;
using System.Text.Json;

namespace producer.Services.ExchangeProviders
{
    public sealed class ExchangeRateAPIFactory : IExchangeFactory
    {
        private readonly string _key;
        public ExchangeRateAPIFactory(string key) => _key = key;

        public IUrlBuilder UrlBuilder() => new ERAF_UrlBuilder();
        public IRequestBuilder RequestBuilder() => new ERAF_RequestBuilder();
        public IResponseBuilder ResponseBuilder() => new ERAF_ResponseBuilder();
    }

    public sealed class ERAF_UrlBuilder : IUrlBuilder
    {
        public string ProvideURL(string key, string from, string to)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return $"https://v6.exchangerate-api.com/v6/{key}/pair/{from}/{to}";
        }
    }

    public sealed class ERAF_RequestBuilder : IRequestBuilder
    {
        public async Task<string> ProvideRequest(HttpClient client, string url, string? key = null)
        {
            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return json;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("exchangerate-api: failed to parse request.", ex);
            }
        }
    }

    public sealed class ERAF_ResponseBuilder : IResponseBuilder
    {
        public Task<ExchangeRate> ProvideResponse(string json, string from, string to)
        {
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
                var element = (JsonElement)dict["conversion_rate"];
                decimal rate = Convert.ToDecimal(element.GetRawText());
                return Task.FromResult(new ExchangeRate(from, to, rate));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("exchangerate-api: failed to parse response.", ex);
            }
        }
    }
}