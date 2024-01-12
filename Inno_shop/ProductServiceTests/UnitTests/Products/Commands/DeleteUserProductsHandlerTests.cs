using ProductService.Application.ProductFeatures.Commands.DeleteUserProducts;
using ProductService.Infrastructure.Interfaces;
using ProductServiceTests.UnitTests.Mocks;

namespace ProductServiceTests.UnitTests.Products.Commands;

[Collection("ProductUnit")]
public class DeleteUserProductsHandlerTests
{
    private readonly IProductRepository mockProductRepo = MockProductRepository.GetProductRepository().Object;
    private readonly DeleteUserProductsHandler handler;

    public DeleteUserProductsHandlerTests() => handler = new(mockProductRepo);

    [Fact]
    public void DeleteValidTest()
    {
        var result = handler.Handle(new DeleteUserProductsCommand(new("236DA01F-9ABD-4d9d-80C7-02AF85C822A2"), new("236DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None).IsCompletedSuccessfully;
        var items = mockProductRepo.GetAllAsync().Result;

        Assert.True(result);
        Assert.Equal(2, items.Count());

        MockProductRepository.ResetData();
    }

    [Fact]
    public void DeleteWithInvalidUserTest()
    {
        var result = handler.Handle(new DeleteUserProductsCommand(new("00000000-9ABD-4d9d-80C7-02AF85C822A2"), new("236DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None).IsFaulted;
        var items = mockProductRepo.GetAllAsync().Result;

        Assert.True(result);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public void DeleteCancelledTest()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => handler.Handle(new DeleteUserProductsCommand(new("236DA01F-9ABD-4d9d-80C7-02AF85C822A2"), new("236DA01F-9ABD-4d9d-80C7-02AF85C822A2")), cts.Token);
        var items = mockProductRepo.GetAllAsync().Result;

        var exception = Assert.ThrowsAsync<TaskCanceledException>(act).Result;
        Assert.Equal("A task was canceled.", exception.Message);
        Assert.Equal(4, items.Count());
    }
}
