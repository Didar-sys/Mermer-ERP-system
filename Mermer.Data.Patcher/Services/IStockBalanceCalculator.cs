using System.Threading;
using System.Threading.Tasks;

namespace Mermer.Data.Postgres.Services;

public interface IStockBalanceCalculator
{
    /// <summary>
    /// Полностью пересчитывает таблицу stock_balances на основе проверенных документов.
    /// </summary>
    Task RecalculateAllBalancesAsync(CancellationToken cancellationToken = default);
}