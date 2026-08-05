namespace Mermer.Api.DTOs
{
    public record UserSessionDto(
    string Id,
    string Username,
    string Name,
    string Role,
    string Token
);
}
