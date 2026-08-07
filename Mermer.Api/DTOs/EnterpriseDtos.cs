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