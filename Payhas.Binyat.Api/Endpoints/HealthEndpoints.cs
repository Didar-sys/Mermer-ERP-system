using Payhas.Binyat.Data.Postgres.Abstractions;

namespace Payhas.Binyat.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/health").WithTags("Health");

        group.MapGet("/", () => Results.Ok(new
        {
            status = "ok",
            time = DateTime.UtcNow,
            service = "Payhas.Binyat.Api"
        }))
        .WithName("HealthRoot");

        group.MapGet("/db", async (IStocksRepository stocks, CancellationToken ct) =>
        {
            try
            {
                await stocks.GetFacetsAsync(new[] { "type" }, ct);
                return Results.Ok(new { status = "ok", db = "reachable" });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Database unreachable",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        })
        .WithName("HealthDatabase");

        return app;
    }
}
