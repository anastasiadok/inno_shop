using ProductService.Application.ProductFeatures.Queries.GetFilteredSortedProducts;
using ProductService.Domain.Entities;
using ProductServiceTests.UnitTests.Mocks;
using Sieve.Exceptions;
using Sieve.Models;

namespace ProductServiceTests.UnitTests.Products.Queries;

public class GetFilteredSortedHandlerTests
{
    [Fact]
    public async Task GetValidTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        GetFilteredSortedProductsHandler handler = new(mockProductRepo, new ApplicationSieveProcessor(new SieveOptionsAccessor()));

        SieveModel model = new()
        {
            PageSize = 3,
            Page = 2,
            Sorts = "Name"
        };

        var result = await handler.Handle(new GetFilteredSortedProductsQuery(model), CancellationToken.None);
        var items = mockProductRepo.GetAllIQueryable().OrderBy(p => p.Name).Chunk(3).ElementAt(1).AsEnumerable();

        Assert.IsType<List<Product>>(result);
        Assert.Equal(items.Count(), result.Count());
        Assert.Equal(items, result);
    }

    [Fact]
    public async Task GetInvalidParamsTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        GetFilteredSortedProductsHandler handler = new(mockProductRepo, new ApplicationSieveProcessor(new SieveOptionsAccessor()));

        SieveModel model = new()
        {
            PageSize = 3,
            Page = 2,
            Sorts = "Nameeeeeeeeeee"
        };

        var act = () => handler.Handle(new GetFilteredSortedProductsQuery(model), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<SieveMethodNotFoundException>(act);
        Assert.Equal("Nameeeeeeeeeee not found.", exception.Message);
    }

    [Fact]
    public async Task GetCancelledTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        GetFilteredSortedProductsHandler handler = new(mockProductRepo, new ApplicationSieveProcessor(new SieveOptionsAccessor()));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        SieveModel model = new()
        {
            PageSize = 3,
            Page = 2,
            Sorts = "Name"
        };

        var act = () => handler.Handle(new GetFilteredSortedProductsQuery(model), cts.Token);

        var exception = await Assert.ThrowsAsync<TaskCanceledException>(act);
        Assert.Equal("A task was canceled.", exception.Message);
    }
}
