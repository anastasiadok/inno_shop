using ProductService.Application.ProductFeatures.Queries.GetFilteredSortedProducts;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using ProductServiceTests.UnitTests.Mocks;
using Sieve.Exceptions;
using Sieve.Models;

namespace ProductServiceTests.UnitTests.Products.Queries;

[Collection("ProductUnit")]
public class GetFilteredSortedHandlerTests
{
    private readonly IProductRepository mockProductRepo = MockProductRepository.GetProductRepository().Object;
    private readonly GetFilteredSortedProductsHandler handler;

    public GetFilteredSortedHandlerTests() => handler = new(mockProductRepo, new ApplicationSieveProcessor(new SieveOptionsAccessor()));

    [Fact]
    public void GetValidTest()
    {
        SieveModel model = new()
        {
            PageSize = 3,
            Page = 2,
            Sorts = "Name"
        };

        var result = handler.Handle(new GetFilteredSortedProductsQuery(model), CancellationToken.None).Result;
        var items = mockProductRepo.GetAllIQueryable().OrderBy(p => p.Name).Chunk(3).ElementAt(1).AsEnumerable();

        Assert.IsType<List<Product>>(result);
        Assert.Equal(items.Count(), result.Count());
        Assert.Equal(items, result);
    }

    [Fact]
    public void GetInvalidParamsTest()
    {
        SieveModel model = new()
        {
            PageSize = 3,
            Page = 2,
            Sorts = "Nameeeeeeeeeee"
        };

        var act = () => handler.Handle(new GetFilteredSortedProductsQuery(model), CancellationToken.None);

        var exception = Assert.ThrowsAsync<SieveMethodNotFoundException>(act).Result;
        Assert.Equal("Nameeeeeeeeeee not found.", exception.Message);
    }

    [Fact]
    public void GetCancelledTest()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        SieveModel model = new()
        {
            PageSize = 3,
            Page = 2,
            Sorts = "Name"
        };

        var act = () => handler.Handle(new GetFilteredSortedProductsQuery(model), cts.Token);

        var exception = Assert.ThrowsAsync<TaskCanceledException>(act).Result;
        Assert.Equal("A task was canceled.", exception.Message);
    }
}
