using Mermer.Api.DTOs;

namespace Mermer.Api.Services;

public interface ISyncService
{
    /// <summary>
    /// Принимает пакет документов от SQLite-клиента, сохраняет в Postgres
    /// и запускает пересчет регистра остатков.
    /// </summary>
    Task<SyncPushResponseDto> ProcessPushAsync(SyncPushRequestDto request, CancellationToken cancellationToken = default);
    Task<SyncPullResponseDto> ProcessPullAsync(CancellationToken cancellationToken = default);
}