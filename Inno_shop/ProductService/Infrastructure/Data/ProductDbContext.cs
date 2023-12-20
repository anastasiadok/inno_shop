using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Data;

public class ProductDbContext: DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
        var dbCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
        if (dbCreator != null)
        {
            if (!dbCreator.CanConnect()) dbCreator.Create();
            if (!dbCreator.HasTables()) dbCreator.CreateTables();
        }
    }
}