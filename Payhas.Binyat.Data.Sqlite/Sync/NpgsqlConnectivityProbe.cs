using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Payhas.Binyat.Data.Sqlite.Sync;

/// <summary>
/// Connectivity probe based on a 2-second Npgsql connection attempt.
/// Cached for 5 seconds to avoid pounding the server when the UI
/// switches between online/offline rapidly.
/// </summary>
public sealed class NpgsqlConnectivityProbe : IConnectivityProbe
{
    private readonly string _connectionString;
    private DateTime _lastCheck = DateTime.MinValue;
    private bool _lastResult;

    public NpgsqlConnectivityProbe(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<bool> IsOnlineAsync(CancellationToken ct = default)
    {
        if (DateTime.UtcNow - _lastCheck < TimeSpan.FromSeconds(5))
            return _lastResult;

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(_connectionString)
            {
                Timeout = 2,
                CommandTimeout = 2
            };
            await using var conn = new NpgsqlConnection(builder.ConnectionString);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(2));
            await conn.OpenAsync(cts.Token);
            _lastResult = true;
        }
        catch
        {
            _lastResult = false;
        }
        finally
        {
            _lastCheck = DateTime.UtcNow;
        }

        return _lastResult;
    }
}
