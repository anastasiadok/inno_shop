using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductService;
using ProductService.Infrastructure.Contexts;

namespace ProductServiceTests.IntegrationTests.Helpers;

public class CustomFactory<TProgram> : WebApplicationFactory<Program> where TProgram : Program
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<ProductDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<ProductDbContext>(options =>
            {
                options.UseNpgsql("Server=localhost;Port=5432;User id=postgres;password=password;database=producttest");
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            using var appContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

            appContext.Database.EnsureCreated();
            SeedData.PopulateTestData(appContext);
        });
    }
}

