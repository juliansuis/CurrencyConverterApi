using System.Text.Json;
using CurrencyConverterApi.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyConverterApi.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly string _apiKey;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

    private const string BaseUrl = "https://v6.exchangerate-api.com/v6";
    private const string RatesCacheKeyPrefix = "rates_";
    private const string SupportedCodesCacheKey = "supported_codes";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ExchangeRateService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _cache = cache;
        _apiKey = configuration["ExchangeRateApiKey"]
            ?? throw new InvalidOperationException("ExchangeRateApiKey is not configured in appsettings.json.");
    }

    public async Task<ExchangeRateApiResponse?> GetRatesAsync(string baseCurrency)
    {
        var cacheKey = RatesCacheKeyPrefix + baseCurrency.ToUpperInvariant();

        if (_cache.TryGetValue(cacheKey, out ExchangeRateApiResponse? cached))
            return cached;

        var url = $"{BaseUrl}/{_apiKey}/latest/{baseCurrency.ToUpperInvariant()}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<ExchangeRateApiResponse>(content, JsonOptions);

        if (data is null || data.Result != "success")
            return data; // return as-is so the controller can inspect ErrorType

        _cache.Set(cacheKey, data, _cacheDuration);
        return data;
    }

    public async Task<List<SupportedCurrency>> GetSupportedCurrenciesAsync()
    {
        if (_cache.TryGetValue(SupportedCodesCacheKey, out List<SupportedCurrency>? cached) && cached is not null)
            return cached;

        var url = $"{BaseUrl}/{_apiKey}/codes";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return new List<SupportedCurrency>();

        var content = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<SupportedCodesApiResponse>(content, JsonOptions);

        if (data is null || data.Result != "success")
            return new List<SupportedCurrency>();

        var currencies = data.SupportedCodes
            .Where(c => c.Count >= 2)
            .Select(c => new SupportedCurrency(c[0], c[1]))
            .ToList();

        _cache.Set(SupportedCodesCacheKey, currencies, _cacheDuration);
        return currencies;
    }
}
