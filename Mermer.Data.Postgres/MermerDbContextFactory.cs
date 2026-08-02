using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mermer.Data.Postgres;

public class MermerDbContextFactory : IDesignTimeDbContextFactory<MermerDbContext>
{
    public MermerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MermerDbContext>();

        // Укажи здесь свою локальную строку подключения к PostgreSQL
        optionsBuilder.UseNpgsql("Host=localhost;Database=mermer_db;Username=postgres;Password=1234");

        return new MermerDbContext(optionsBuilder.Options);
    }
}