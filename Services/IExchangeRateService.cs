using CurrencyConverterApi.Models;

namespace CurrencyConverterApi.Services;

public interface IExchangeRateService
{
    Task<ExchangeRateApiResponse?> GetRatesAsync(string baseCurrency);
    Task<List<SupportedCurrency>> GetSupportedCurrenciesAsync();
}

public record SupportedCurrency(string Code, string Name);
