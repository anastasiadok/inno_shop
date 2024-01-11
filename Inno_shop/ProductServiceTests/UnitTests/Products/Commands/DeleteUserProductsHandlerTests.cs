using ProductService.Application.ProductFeatures.Commands.DeleteUserProducts;
using ProductServiceTests.UnitTests.Mocks;

namespace ProductServiceTests.UnitTests.Products.Commands;

public class DeleteUserProductsHandlerTests
{
    [Fact]
    public async Task DeleteValidTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        DeleteUserProductsHandler handler = new(mockProductRepo);

        var result = handler.Handle(new DeleteUserProductsCommand(new("236DA01F-9ABD-4d9d-80C7-02AF85C822A2"), new("236DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None).IsCompletedSuccessfully;
        var items = await mockProductRepo.GetAllAsync();

        Assert.True(result);
        Assert.Equal(2, items.Count());
    }

    [Fact]
    public async Task DeleteWithInvalidUserTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        DeleteUserProductsHandler handler = new(mockProductRepo);

        var result = handler.Handle(new DeleteUserProductsCommand(new("00000000-9ABD-4d9d-80C7-02AF85C822A2"), new("236DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None).IsFaulted;
        var items = await mockProductRepo.GetAllAsync();

        Assert.True(result);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public async Task DeleteCancelledTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        DeleteUserProductsHandler handler = new(mockProductRepo);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => handler.Handle(new DeleteUserProductsCommand(new("236DA01F-9ABD-4d9d-80C7-02AF85C822A2"), new("236DA01F-9ABD-4d9d-80C7-02AF85C822A2")), cts.Token);
        var items = await mockProductRepo.GetAllAsync();

        var exception = await Assert.ThrowsAsync<TaskCanceledException>(act);
        Assert.Equal("A task was canceled.", exception.Message);
        Assert.Equal(4, items.Count());
    }
}
