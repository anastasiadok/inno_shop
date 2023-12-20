using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.ProductFeatures.Commands.AddProduct;
using ProductService.Application.ProductFeatures.Queries.GetFilteredSortedProducts;
using ProductService.Infrastructure.Data;
using ProductService.Presentation.Middlewares;
using Sieve.Services;
using System.Reflection;

namespace ProductService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var mode = Environment.GetEnvironmentVariable("MODE");
        var conectionStringName = mode == "container" ? "PostgreSqlDocker" : "PostgreSql";
        builder.Services.AddDbContext<ProductDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString(conectionStringName)));

        builder.Services.AddValidatorsFromAssemblies(new[] { Assembly.GetExecutingAssembly() });
        builder.Services.AddControllers()
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateProductDtoValidator>());

        builder.Services.AddScoped<SieveProcessor, ApplicationSieveProcessor>();
     
        builder.Services.AddCors();

        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader());
       
        app.UseHttpsRedirection();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}