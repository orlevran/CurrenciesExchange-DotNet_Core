using producer.Models;

namespace producer.Services.ExchangeProviders
{
    public interface IExchangeFactory
    {
        IUrlBuilder UrlBuilder();
        IRequestBuilder RequestBuilder();
        IResponseBuilder ResponseBuilder();
    }

    public interface IUrlBuilder
    {
        string ProvideURL(string key, string from, string to);
    }

    public interface IRequestBuilder
    {
        Task<string> ProvideRequest(HttpClient client, string url, string? key = null);
    }

    public interface IResponseBuilder
    {
        Task<ExchangeRate> ProvideResponse(string json, string from, string to);
    }

    public interface IExchangeClient
    {
        Task<ExchangeRate> GetRatesAsync(string key, string from, string to, CancellationToken ct = default);
    }

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

        public async Task<ExchangeRate> GetRatesAsync(string key, string from, string to, CancellationToken ct = default)
        {
            try
            {
                string url = urlBuilder.ProvideURL(key, from, to);
                string json = await requestBuilder.ProvideRequest(client, url);
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