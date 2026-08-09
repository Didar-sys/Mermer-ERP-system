namespace Mermer.Api.DTOs;

public record InvoiceDto(
    string Id,
    string Code,
    string InvoiceType,
    DateTime Date,
    bool IsCompleted,
    string PartnerId,
    string Description
);