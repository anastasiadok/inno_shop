using ProductService.Application.ProductFeatures.Queries.GetByIdProduct;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductServiceTests.UnitTests.Mocks;

namespace ProductServiceTests.UnitTests.Products.Queries;

[Collection("ProductUnit")]
public class GetByIdHandlerTests
{
    private readonly GetByIdProductHandler handler = new(MockProductRepository.GetProductRepository().Object);

    [Fact]
    public void GetValidTest()
    {
        Guid idToGet = new("336DA01F-9ABD-4d9d-80C7-02AF85C822A1");
        var result = handler.Handle(new GetByIdProductQuery(idToGet), CancellationToken.None).Result;

        Assert.IsType<Product>(result);
        Assert.Equal(idToGet, result.Id);
    }

    [Fact]
    public void GetNotExistingTest()
    {
        Guid idToGet = new("00000000-9ABD-4d9d-80C7-02AF85C822A1");
        var act = () => handler.Handle(new GetByIdProductQuery(idToGet), CancellationToken.None);

        var exception = Assert.ThrowsAsync<NotFoundException>(act).Result;
        Assert.Equal("Product not found.", exception.Message);
    }

    [Fact]
    public void GetCancelledTest()
    {
        Guid idToGet = new("336DA01F-9ABD-4d9d-80C7-02AF85C822A1");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => handler.Handle(new GetByIdProductQuery(idToGet), cts.Token);

        var exception = Assert.ThrowsAsync<TaskCanceledException>(act).Result;
        Assert.Equal("A task was canceled.", exception.Message);
    }
}
