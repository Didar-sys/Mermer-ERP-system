using System.Threading;
using System.Threading.Tasks;

namespace Mermer.Data.Sqlite.Sync;

/// <summary>
/// Lightweight "is the central PostgreSQL reachable?" check.
/// Default implementation simply opens an Npgsql connection with a short
/// timeout; in production it can be replaced with a proper health probe.
/// </summary>
public interface IConnectivityProbe
{
    Task<bool> IsOnlineAsync(CancellationToken ct = default);
}
