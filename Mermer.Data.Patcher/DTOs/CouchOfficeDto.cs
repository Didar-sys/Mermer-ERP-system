using System;
using System.Text.Json.Serialization;

namespace Mermer.Data.Patcher.DTOs;

public class CouchOfficeDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

    [JsonPropertyName("isDisabled")]
    public bool IsDisabled { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; set; }
}