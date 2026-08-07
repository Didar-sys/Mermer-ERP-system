namespace Mermer.Api.DTOs;

public record OfficeDto(
    string Id,
    string Name,
    string? Description
);

public record WarehouseDetailsDto(
    string Id,
    string Name,
    string? OfficeId,
    string? Description
);

public record CurrencyDto(
    string Id,
    string Name
);

public record StockDetailsDto(
    string Id,
    string Name
);

public record PartnerDetailsDto(
    string Id,
    string Name
);