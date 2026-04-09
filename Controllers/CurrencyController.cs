using CurrencyConverterApi.Models;
using CurrencyConverterApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverterApi.Controllers;

[ApiController]
[Route("api/currency")]
public class CurrencyController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateService;

    public CurrencyController(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    /// <summary>
    /// Returns all available exchange rates for a given base currency.
    /// </summary>
    [HttpGet("rates/{baseCurrency}")]
    public async Task<IActionResult> GetRates(string baseCurrency)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency) || baseCurrency.Length != 3)
            return BadRequest(new { error = "Currency code must be a 3-letter ISO 4217 code (e.g. USD, EUR)." });

        var data = await _exchangeRateService.GetRatesAsync(baseCurrency);

        if (data is null)
            return StatusCode(502, new { error = "Failed to reach the exchange rate provider." });

        if (data.Result != "success")
        {
            if (data.ErrorType == "unsupported-code")
                return BadRequest(new { error = $"'{baseCurrency.ToUpperInvariant()}' is not a supported currency code." });

            return StatusCode(502, new { error = "Exchange rate provider returned an error.", detail = data.ErrorType });
        }

        return Ok(new
        {
            baseCurrency = data.BaseCode,
            lastUpdated = data.TimeLastUpdateUtc,
            rates = data.ConversionRates
        });
    }

    /// <summary>
    /// Converts an amount from one currency to another.
    /// </summary>
    [HttpGet("convert")]
    public async Task<IActionResult> Convert(
        [FromQuery] string from,
        [FromQuery] string to,
        [FromQuery] decimal amount)
    {
        if (string.IsNullOrWhiteSpace(from) || from.Length != 3)
            return BadRequest(new { error = "'from' must be a 3-letter ISO 4217 currency code." });

        if (string.IsNullOrWhiteSpace(to) || to.Length != 3)
            return BadRequest(new { error = "'to' must be a 3-letter ISO 4217 currency code." });

        if (amount <= 0)
            return BadRequest(new { error = "'amount' must be greater than zero." });

        var data = await _exchangeRateService.GetRatesAsync(from);

        if (data is null)
            return StatusCode(502, new { error = "Failed to reach the exchange rate provider." });

        if (data.Result != "success")
        {
            if (data.ErrorType == "unsupported-code")
                return BadRequest(new { error = $"'{from.ToUpperInvariant()}' is not a supported currency code." });

            return StatusCode(502, new { error = "Exchange rate provider returned an error.", detail = data.ErrorType });
        }

        var toUpper = to.ToUpperInvariant();

        if (!data.ConversionRates.TryGetValue(toUpper, out var exchangeRate))
            return BadRequest(new { error = $"'{toUpper}' is not a supported target currency code." });

        var convertedAmount = Math.Round(amount * exchangeRate, 4);

        return Ok(new ConvertResponse
        {
            From = data.BaseCode,
            To = toUpper,
            Amount = amount,
            ConvertedAmount = convertedAmount,
            ExchangeRate = exchangeRate,
            Timestamp = data.TimeLastUpdateUtc
        });
    }

    /// <summary>
    /// Returns the list of all supported currency codes and their names.
    /// </summary>
    [HttpGet("supported")]
    public async Task<IActionResult> GetSupported()
    {
        var currencies = await _exchangeRateService.GetSupportedCurrenciesAsync();

        if (currencies.Count == 0)
            return StatusCode(502, new { error = "Failed to retrieve supported currencies from the exchange rate provider." });

        return Ok(new { count = currencies.Count, currencies });
    }
}
