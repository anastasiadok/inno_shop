using ProductService.Application.ProductFeatures.Queries.GetAllProducts;
using ProductService.Domain.Entities;
using ProductServiceTests.UnitTests.Mocks;

namespace ProductServiceTests.UnitTests.Products.Queries;

[Collection("ProductUnit")]
public class GetAllHandlerTests
{
    private readonly GetAllProductsHandler handler = new(MockProductRepository.GetProductRepository().Object);

    [Fact]
    public void GetValidTest()
    {
        var result = handler.Handle(new GetAllProductsQuery(), CancellationToken.None).Result;

        Assert.IsType<List<Product>>(result);
        Assert.Equal(4, result.Count());
    }

    [Fact]
    public void GetCancelledTest()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => handler.Handle(new GetAllProductsQuery(), cts.Token);

        var exception = Assert.ThrowsAsync<TaskCanceledException>(act).Result;
        Assert.Equal("A task was canceled.", exception.Message);
    }
}
