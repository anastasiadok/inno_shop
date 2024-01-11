using ProductService.Application.ProductFeatures.Queries.GetByIdProduct;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductServiceTests.UnitTests.Mocks;

namespace ProductServiceTests.UnitTests.Products.Queries;

public class GetByIdHandlerTests
{
    [Fact]
    public async Task GetValidTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        GetByIdProductHandler handler = new(mockProductRepo);

        Guid idToGet = new("336DA01F-9ABD-4d9d-80C7-02AF85C822A1");
        var result = await handler.Handle(new GetByIdProductQuery(idToGet), CancellationToken.None);

        Assert.IsType<Product>(result);
        Assert.Equal(idToGet, result.Id);
    }

    [Fact]
    public async Task GetNotExistingTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        GetByIdProductHandler handler = new(mockProductRepo);

        Guid idToGet = new("00000000-9ABD-4d9d-80C7-02AF85C822A1");
        var act = () => handler.Handle(new GetByIdProductQuery(idToGet), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("Product not found.", exception.Message);
    }

    [Fact]
    public async Task GetCancelledTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        GetByIdProductHandler handler = new(mockProductRepo);

        Guid idToGet = new("336DA01F-9ABD-4d9d-80C7-02AF85C822A1");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => handler.Handle(new GetByIdProductQuery(idToGet), cts.Token);

        var exception = await Assert.ThrowsAsync<TaskCanceledException>(act);
        Assert.Equal("A task was canceled.", exception.Message);
    }
}
