namespace Mermer.Api.DTOs;

public record StockSlipDto(
    string Id,
    string Code,
    string SlipType,
    DateTime Date,
    bool IsCompleted,
    decimal DisplayTotal,
    string Description
);