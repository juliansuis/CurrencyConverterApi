using System.Text.Json.Serialization;

namespace CurrencyConverterApi.Models;

public class SupportedCodesApiResponse
{
    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;

    [JsonPropertyName("error-type")]
    public string? ErrorType { get; set; }

    /// <summary>
    /// Each element is a two-item array: ["USD", "US Dollar"]
    /// </summary>
    [JsonPropertyName("supported_codes")]
    public List<List<string>> SupportedCodes { get; set; } = new();
}
