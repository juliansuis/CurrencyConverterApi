namespace CurrencyConverterApi.Models;

public class ConvertResponse
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public string Timestamp { get; set; } = string.Empty;
}
