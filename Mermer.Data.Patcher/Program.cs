using Mermer.Data.Patcher.Services;
using Mermer.Data.Postgres;
using Mermer.Data.Postgres.Entities;
using Mermer.Data.Postgres.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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

        // 1. Очищаем таблицы перед импортом (добавили stock_slips и stock_slip_lines)
        await dbContext.Database.ExecuteSqlRawAsync(@"
            TRUNCATE TABLE 
                offices, partners, warehouses, depositories, currencies, currency_rates, users, 
                stocks, stock_units, stock_prices, 
                invoices, invoice_lines, invoice_payments, invoice_currency_convertions, 
                invoice_stock_unit_convertions, invoice_discounts, invoice_overheads,
                stock_slips, stock_slip_lines 
            CASCADE;");

        // 2. Инициализируем сервисы
        var partnerImporter = new PartnerImportService(dbContext);
        var enterpriseImporter = new EnterpriseImportService(dbContext);
        var nomenclatureImporter = new NomenclatureImportService(dbContext);
        var commerceImporter = new CommerceImportService(dbContext);

        // 3. Этап 1: Базовые справочники (РАСКОММЕНТИРОВАНО!)
        Console.WriteLine("\n--- Этап 1: Базовые справочники ---");
        await partnerImporter.MigratePartnersAsync(jsonFilePath);
        await enterpriseImporter.MigrateOfficesAsync(jsonFilePath);
        await enterpriseImporter.MigrateWarehousesAsync(jsonFilePath);
        await enterpriseImporter.MigrateDepositoriesAsync(jsonFilePath);
        await enterpriseImporter.MigrateCurrenciesAsync(jsonFilePath);
        await enterpriseImporter.MigrateUsersAsync(jsonFilePath);

        Console.WriteLine(new string('-', 40));

        // 4. Этап 2: Номенклатура
        Console.WriteLine("\n--- Этап 2: Номенклатура ---");
        await nomenclatureImporter.MigrateStocksAsync(jsonFilePath);

        Console.WriteLine(new string('-', 40));

        // 5. Этап 3: Документы (Накладные + Складские ордера)
        Console.WriteLine("\n--- Этап 3: Документы ---");
        await commerceImporter.MigrateInvoicesAsync(jsonFilePath);
        await commerceImporter.MigrateStockSlipsAsync(jsonFilePath); // Вызываем импорт StockSlips!

        // 6. Итоговая проверка количества записей в Postgres
        var partnersCount = await dbContext.Partners.CountAsync();
        var officesCount = await dbContext.Offices.CountAsync();
        var warehousesCount = await dbContext.Warehouses.CountAsync();
        var depositoriesCount = await dbContext.Depositories.CountAsync();
        var currenciesCount = await dbContext.Currencies.CountAsync();
        var ratesCount = await dbContext.CurrencyRates.CountAsync();
        var usersCount = await dbContext.Users.CountAsync();

        var stocksCount = await dbContext.Stocks.CountAsync();
        var stockUnitsCount = await dbContext.StockUnits.CountAsync();
        var stockPricesCount = await dbContext.StockPrices.CountAsync();

        var invoicesCount = await dbContext.Set<InvoiceEntity>().CountAsync();
        var invoiceLinesCount = await dbContext.Set<InvoiceLineEntity>().CountAsync();
        var stockSlipsCount = await dbContext.Set<StockSlipEntity>().CountAsync();
        var stockSlipLinesCount = await dbContext.Set<StockSlipLineEntity>().CountAsync();

        Console.WriteLine("\n================ РЕЗУЛЬТАТ ИМПОРТА ================");

        Console.WriteLine("--- Этап 1: Базовые справочники ---");
        Console.WriteLine($"Записей Partner (partners):       {partnersCount}");
        Console.WriteLine($"Записей Office (offices):         {officesCount}");
        Console.WriteLine($"Записей Warehouse (warehouses):   {warehousesCount}");
        Console.WriteLine($"Записей Depository (depositories):{depositoriesCount}");
        Console.WriteLine($"Записей Currency (currencies):    {currenciesCount}");
        Console.WriteLine($"Записей CurrencyRate (rates):     {ratesCount}");
        Console.WriteLine($"Записей User (users):             {usersCount}");

        Console.WriteLine("\n--- Этап 2: Номенклатура ---");
        Console.WriteLine($"Записей Stock (stocks):           {stocksCount}");
        Console.WriteLine($"Записей StockUnit (units):        {stockUnitsCount}");
        Console.WriteLine($"Записей StockPrice (prices):      {stockPricesCount}");

        Console.WriteLine("\n--- Этап 3: Документы ---");
        Console.WriteLine($"Записей Invoice (invoices):       {invoicesCount}");
        Console.WriteLine($"Записей InvoiceLine (lines):      {invoiceLinesCount}");
        Console.WriteLine($"Записей StockSlip (slips):        {stockSlipsCount}");
        Console.WriteLine($"Записей StockSlipLine (lines):    {stockSlipLinesCount}");

        Console.WriteLine("==================================================");

        // 7. ФИНАЛЬНЫЙ ЭТАП: Расчет регистра остатков
        Console.WriteLine("\n==========================================");
        Console.WriteLine("Запуск расчета накопительного регистра остатков...");
        Console.WriteLine("==========================================");

        var calculator = new StockBalanceCalculator(dbContext);
        await calculator.RecalculateAllBalancesAsync();

        Console.WriteLine("\n[УСПЕХ] Миграция данных полностью завершена!");

        Console.WriteLine("\nНажми любую клавишу для выхода...");
        Console.ReadKey();
    }
}