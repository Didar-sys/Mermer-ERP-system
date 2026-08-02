namespace Mermer.Api.DTOs;

/// <summary>
/// Пакет данных, отправляемый офлайн-клиентом на сервер Mermer.Api
/// </summary>
public class SyncPushRequestDto
{
    /// <summary>
    /// Уникальный идентификатор устройства/клиента (например, GUID терминала)
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Новые или измененные накладные (Invoices)
    /// </summary>
    public List<InvoiceSyncDto> Invoices { get; set; } = new();

    /// <summary>
    /// Новые или измененные складские ордера/акты (StockSlips)
    /// </summary>
    public List<StockSlipSyncDto> StockSlips { get; set; } = new();
}

public class InvoiceSyncDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string InvoiceType { get; set; } = string.Empty; // e.g., "Sale", "Purchase"
    public DateTime Date { get; set; }
    public Guid? ClientId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Description { get; set; }
    public List<InvoiceLineSyncDto> Lines { get; set; } = new();
}

public class InvoiceLineSyncDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class StockSlipSyncDto
{
    public Guid Id { get; set; }
    public string SlipNumber { get; set; } = string.Empty;
    public string SlipType { get; set; } = string.Empty; // e.g., "Incoming", "Outgoing"
    public DateTime Date { get; set; }
    public Guid StockId { get; set; }
    public string? Description { get; set; }
    public List<StockSlipLineSyncDto> Lines { get; set; } = new();
}

public class StockSlipLineSyncDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
}

/// <summary>
/// Результат обработки синхронизации сервером
/// </summary>
public class SyncPushResponseDto
{
    public bool Success { get; set; }
    public int ProcessedInvoicesCount { get; set; }
    public int ProcessedStockSlipsCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
}