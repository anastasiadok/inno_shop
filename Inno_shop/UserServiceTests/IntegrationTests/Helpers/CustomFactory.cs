using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserService;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Contexts;

namespace UserServiceTests.IntegrationTests.Helpers;

public class CustomFactory<TProgram> : WebApplicationFactory<Program> where TProgram : Program
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<UserDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
            services.Remove(descriptor);

            descriptor = new(typeof(IEmailService), typeof(TestEmailService), ServiceLifetime.Scoped);
            services.Add(descriptor);

            services.AddDbContext<UserDbContext>(options =>
            {
                options.UseNpgsql("Server=localhost;Port=5432;User id=postgres;password=password;database=usertest");
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            using var appContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

            appContext.Database.EnsureCreated();
            SeedData.PopulateTestData(appContext);
        });
    }
}

