using System;

namespace Mermer.Data.Postgres.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsDisabled { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}