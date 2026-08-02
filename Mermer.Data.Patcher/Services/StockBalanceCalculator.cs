using Mermer.Data.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mermer.Data.Postgres.Services;

public class StockBalanceCalculator : IStockBalanceCalculator
{
    private readonly MermerDbContext _db;

    public StockBalanceCalculator(MermerDbContext db)
    {
        _db = db;
    }

    public async Task RecalculateAllBalancesAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("--> [BALANCE CALCULATOR] Начинаем пересчет регистра stock_balances...");

        // 1. Извлекаем движения по Инвойсам (проверенные, с указанным складом и товаром)
        var invoiceMovements = await _db.InvoiceLines
            .AsNoTracking()
            .Where(l => l.Invoice != null
                     && l.Invoice.IsCompleted
                     && !l.Invoice.IsDisabled
                     && l.Invoice.WarehouseId.HasValue
                     && l.StockId.HasValue) // <-- Фильтрация null StockId
            .Select(l => new MovementRecord(
                l.Invoice!.WarehouseId!.Value,
                l.StockId!.Value, // <-- Аргумент 2: использование .Value для приведения Guid? к Guid
                l.Quantity,
                IsIncomeInvoice(l.Invoice.InvoiceType)
            ))
            .ToListAsync(cancellationToken);

        // 2. Извлекаем движения по Складским ордерам (проверенные, с указанным складом и товаром)
        var slipMovements = await _db.StockSlipLines
            .AsNoTracking()
            .Where(l => l.StockSlip != null
                     && l.StockSlip.IsCompleted
                     && l.StockSlip.WarehouseId.HasValue
                     && l.StockId.HasValue) // <-- Фильтрация null StockId
            .Select(l => new MovementRecord(
                l.StockSlip!.WarehouseId!.Value,
                l.StockId!.Value, // <-- Аргумент 2: использование .Value для приведения Guid? к Guid
                l.Quantity,
                l.StockSlip.IsStockIncome
            ))
            .ToListAsync(cancellationToken);

        // 3. Объединяем все движения в один поток
        var allMovements = invoiceMovements.Concat(slipMovements);

        // 4. Группируем по составному ключу (WarehouseId, StockId) и считаем суммы
        var aggregatedBalances = allMovements
            .GroupBy(m => new { m.WarehouseId, m.StockId })
            .Select(g => new StockBalanceEntity
            {
                WarehouseId = g.Key.WarehouseId,
                StockId = g.Key.StockId,
                Income = g.Where(x => x.IsIncome).Sum(x => x.Amount),
                Expense = g.Where(x => !x.IsIncome).Sum(x => x.Amount),
                UpdatedAt = DateTimeOffset.UtcNow
            })
            .ToList();

        Console.WriteLine($"--> [BALANCE CALCULATOR] Рассчитано пар (Склад + Товар): {aggregatedBalances.Count}");

        // 5. Очищаем старые записи и атомарно записываем новые
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE stock_balances;", cancellationToken);

            await _db.StockBalances.AddRangeAsync(aggregatedBalances, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            Console.WriteLine("--> [BALANCE CALCULATOR] Регистр stock_balances успешно обновлен!");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            Console.WriteLine($"--> [ERROR] Ошибка при обновлении stock_balances: {ex.Message}");
            throw;
        }
    }

    private static bool IsIncomeInvoice(string invoiceType)
    {
        return invoiceType.Equals("Purchase", StringComparison.OrdinalIgnoreCase) ||
               invoiceType.Equals("SalesReturn", StringComparison.OrdinalIgnoreCase) ||
               invoiceType.Equals("Opening", StringComparison.OrdinalIgnoreCase);
    }

    private record MovementRecord(Guid WarehouseId, Guid StockId, decimal Amount, bool IsIncome);
}