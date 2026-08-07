namespace Mermer.Api.DTOs;

public record FundsActionDto(
    string Id,
    string Code,
    DateTime Date,
    string FundsSlipType,
    string Description
);