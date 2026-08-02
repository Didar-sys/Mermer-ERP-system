using System;
using System.Collections.Generic;

namespace Mermer.Api.DTOs;

public class StockDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal Price { get; set; }
}

public class PartnerDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class WarehouseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class SyncPullResponseDto
{
    public List<StockDto> Stocks { get; set; } = new();
    public List<PartnerDto> Partners { get; set; } = new();
    public List<WarehouseDto> Warehouses { get; set; } = new();
    public DateTimeOffset ServerTime { get; set; } = DateTimeOffset.UtcNow;
}