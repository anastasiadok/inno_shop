using Microsoft.Extensions.DependencyInjection;
using ProductService;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Contexts;

namespace ProductServiceTests.IntegrationTests.Helpers;

public static class SeedData
{
    public static void ResetData()
    {
        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        PopulateTestData(appContext);
    }

    public static void PopulateTestData(ProductDbContext dbContext)
    {
        dbContext.Products.RemoveRange(dbContext.Products.ToList());

        dbContext.Products.AddRange(
        new Product
        {
            Id = new("236DA01F-9ABD-4d9d-80C7-02AF85C822A3"),
            Name = "Programming",
            CreationDate = DateTime.UtcNow,
            CreatorId = new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"),
            Description = "do it 3 times a day and spina ne budet bolet",
            Price = 15,
            IsAvailible = true
        },
        new Product
        {
            Id = new("336DA01F-9ABD-4d9d-80C7-02AF85C822A1"),
            Name = "Bolshie Gorodaa",
            CreationDate = DateTime.UtcNow,
            CreatorId = new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"),
            Description = "Pustie poezdaaa",
            Price = 10000,
            IsAvailible = false
        });

        dbContext.SaveChanges();
    }
}
