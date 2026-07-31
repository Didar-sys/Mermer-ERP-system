using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mermer.Data.Postgres;
using Mermer.Data.Postgres.Entities;
using Mermer.Data.Patcher.DTOs;

namespace Mermer.Data.Patcher.Services;

public class PartnerImportService
{
    private readonly MermerDbContext _dbContext;

    public PartnerImportService(MermerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Вспомогательный метод: ищет первые N объектов с docType == "Partner"
    /// </summary>
    public async Task DumpPartnerJsonAsync(string jsonFilePath, int takeCount = 3)
    {
        Console.WriteLine($"--- Поиск {takeCount} записей с docType == Partner ---");

        using var stream = File.OpenRead(jsonFilePath);
        using var reader = new StreamReader(stream);

        string? line;
        int found = 0;

        while ((line = await reader.ReadLineAsync()) != null && found < takeCount)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;

            string? docType = null;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("docType", out var dt) && dt.ValueKind == JsonValueKind.String)
                {
                    docType = dt.GetString();
                }
                else if (root.TryGetProperty("patch", out var patch) &&
                         patch.ValueKind == JsonValueKind.Object &&
                         patch.TryGetProperty("docType", out var pdt) &&
                         pdt.ValueKind == JsonValueKind.String)
                {
                    docType = pdt.GetString();
                }
            }

            if (docType == "Partner")
            {
                found++;
                Console.WriteLine($"\n[Найден Partner #{found}]:");
                Console.WriteLine(line);
            }
        }

        if (found == 0)
        {
            Console.WriteLine("Записей Partner не найдено!");
        }
    }

    /// <summary>
    /// Полный импорт справочника Partner в PostgreSQL
    /// </summary>
    public async Task MigratePartnersAsync(string jsonFilePath)
    {
        Console.WriteLine("Начинаем импорт справочника Partner...");

        using var stream = File.OpenRead(jsonFilePath);
        using var reader = new StreamReader(stream);

        var partnersBatch = new List<PartnerEntity>();
        string? line;
        int totalImported = 0;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("docType", out var dt) &&
                dt.ValueKind == JsonValueKind.String &&
                dt.GetString() == "Partner")
            {
                var couchPartner = JsonSerializer.Deserialize<CouchPartnerDto>(line);

                if (couchPartner != null)
                {
                    var pgPartner = new PartnerEntity
                    {
                        Id = couchPartner.Id,
                        Code = couchPartner.Code ?? string.Empty,
                        Name = couchPartner.Name ?? string.Empty,
                        Phone = couchPartner.Phone,
                        Address = couchPartner.Address,
                        IsDisabled = couchPartner.IsDisabled
                    };

                    partnersBatch.Add(pgPartner);

                    if (partnersBatch.Count >= 500)
                    {
                        await _dbContext.AddRangeAsync(partnersBatch);
                        await _dbContext.SaveChangesAsync();

                        totalImported += partnersBatch.Count;
                        Console.WriteLine($"Сохранено контрагентов: {totalImported}...");

                        partnersBatch.Clear();
                    }
                }
            }
        }

        if (partnersBatch.Any())
        {
            await _dbContext.AddRangeAsync(partnersBatch);
            await _dbContext.SaveChangesAsync();
            totalImported += partnersBatch.Count;
        }

        Console.WriteLine($"Готово! Всего импортировано Partner: {totalImported}");
    }
}