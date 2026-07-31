using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mermer.Data.Postgres;

namespace Mermer.Data.Patcher;

class Program
{
    static async Task Main(string[] args)
    {
        string jsonFilePath = @"D:\Программирование\binyat_export.json";

        var optionsBuilder = new DbContextOptionsBuilder<MermerDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=mermer_db;Username=postgres;Password=1234");

        using var dbContext = new MermerDbContext(optionsBuilder.Options);

        Console.WriteLine("Проверяем структуру БД и очищаем старые данные...");

        // 1. Создаст ТАБЛИЦЫ, если их нет (саму базу mermer_db ты уже создал на Шаге 1)
        await dbContext.Database.EnsureCreatedAsync();

        // 2. Очищаем таблицу перед импортом (чтобы можно было запускать код много раз без ошибок дубликатов)
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE partners CASCADE;");

        var importer = new PartnerImportService(dbContext);

        // 3. Запускаем импорт
        await importer.MigratePartnersAsync(jsonFilePath);

        // 4. Проверяем количество записей
        var count = await dbContext.Partners.CountAsync();
        Console.WriteLine($"\nУспешно! Записей в БД Postgres (таблица partners): {count}");

        Console.WriteLine("\nНажми любую клавишу для выхода...");
        Console.ReadKey();
    }
}