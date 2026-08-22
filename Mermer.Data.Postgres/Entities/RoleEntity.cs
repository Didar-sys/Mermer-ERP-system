using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Mermer.Data.Postgres.Entities;

[Table("roles")]
public class RoleEntity
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    // Колонка в БД хранит чистый JSON-текст
    [Column("authorizations")]
    public string? AuthorizationsJson { get; set; }

    [NotMapped]
    public Dictionary<string, int> Authorizations
    {
        get
        {
            if (string.IsNullOrWhiteSpace(AuthorizationsJson))
                return new Dictionary<string, int>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, int>>(AuthorizationsJson) ?? new Dictionary<string, int>();
            }
            catch
            {
                return new Dictionary<string, int>();
            }
        }
        set
        {
            AuthorizationsJson = value != null ? JsonSerializer.Serialize(value) : "{}";
        }
    }

    [Column("is_disabled")]
    public bool IsDisabled { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}