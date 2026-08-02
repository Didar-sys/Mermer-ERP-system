using System.Text.Json.Serialization;

namespace Mermer.Data.Patcher.DTOs;

public class CurrencyDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("decimals")]
    public int? Decimals { get; set; }

    [JsonPropertyName("isDefault")]
    public bool? IsDefault { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("isDisabled")]
    public bool? IsDisabled { get; set; }
}