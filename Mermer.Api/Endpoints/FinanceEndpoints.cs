using Mermer.Api.DTOs;
using Mermer.Data.Postgres;
using Microsoft.EntityFrameworkCore;

namespace Mermer.Api.Endpoints;

public static class FinanceEndpoints
{
    public static void MapFinanceEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/finance").WithTags("Finance");

        // --- ФИНАНСОВЫЕ ОПЕРАЦИИ (FUNDS ACTIONS / SLIPS) ---
        group.MapGet("/actions", async (MermerDbContext db) =>
        {
            var actions = await db.FundsSlips
                .AsNoTracking()
                .Select(s => new FundsActionDto(
                    s.Id.ToString(),
                    s.Code,
                    s.Date,
                    s.FundsSlipType,
                    s.Description
                ))
                .ToListAsync();

            return Results.Ok(actions);
        })
        .WithName("GetFundsActions")
        .WithSummary("Получить список финансовых операций (ордеров)");
    }
}