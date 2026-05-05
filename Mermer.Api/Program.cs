using Microsoft.OpenApi.Models;
using Mermer.Api.Endpoints;
using Mermer.Data.Postgres;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Connection string 'Postgres' is not configured. " +
        "Set it in appsettings.json or via environment variable " +
        "ConnectionStrings__Postgres.");

builder.Services.AddPayhasPostgres(connectionString);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? new[] { "*" };

        if (origins.Length == 1 && origins[0] == "*")
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payhas Binyat ERP API",
        Version = "v1",
        Description =
            "HTTP API over the new PostgreSQL data layer. " +
            "Replaces the legacy Couchbase access for the WPF client.",
        Contact = new OpenApiContact { Name = "Payhas Binyat" }
    });
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNamingPolicy =
        System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "Payhas Binyat API v1");
    o.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.MapHealthEndpoints();
app.MapStocksEndpoints();
app.MapInvoicesEndpoints();
app.MapBalancesEndpoints();

app.Run();
