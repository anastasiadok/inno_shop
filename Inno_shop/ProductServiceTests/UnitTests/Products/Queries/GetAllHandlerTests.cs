using ProductService.Application.ProductFeatures.Queries.GetAllProducts;
using ProductService.Domain.Entities;
using ProductServiceTests.UnitTests.Mocks;

namespace ProductServiceTests.UnitTests.Products.Queries;

public class GetAllHandlerTests
{
    [Fact]
    public async Task GetValidTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        GetAllProductsHandler handler = new(mockProductRepo);

        var result = await handler.Handle(new GetAllProductsQuery(), CancellationToken.None);

        Assert.IsType<List<Product>>(result);
        Assert.Equal(4, result.Count());
    }

    [Fact]
    public async Task GetCancelledTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        GetAllProductsHandler handler = new(mockProductRepo);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => handler.Handle(new GetAllProductsQuery(), cts.Token);

        var exception = await Assert.ThrowsAsync<TaskCanceledException>(act);
        Assert.Equal("A task was canceled.", exception.Message);
    }
}
