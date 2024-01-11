using ProductService.Application.ProductFeatures.Commands.DeleteProduct;
using ProductService.Domain.Exceptions;
using ProductServiceTests.UnitTests.Mocks;

namespace ProductServiceTests.UnitTests.Products.Commands;

public class DeleteHandlerTests
{
    [Fact]
    public async Task DeleteValidTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        DeleteProductHandler handler = new(mockProductRepo);

        var result = handler.Handle(new DeleteProductCommand(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"), new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None).IsCompletedSuccessfully;
        var items = await mockProductRepo.GetAllAsync();

        Assert.True(result);
        Assert.Equal(3, items.Count());
    }

    [Fact]
    public async Task DeleteWithInvaidUserTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        DeleteProductHandler handler = new(mockProductRepo);

        var act = () => handler.Handle(new DeleteProductCommand(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"), new("00000000-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None);
        var items = await mockProductRepo.GetAllAsync();

        var exception = await Assert.ThrowsAsync<UserAccessException>(act);
        Assert.Equal("User can only manage their own products.", exception.Message);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public async Task DeleteNotExistingTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        DeleteProductHandler handler = new(mockProductRepo);

        var act = () => handler.Handle(new DeleteProductCommand(new("00000000-9ABD-4d9d-80C7-02AF85C822A1"), new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None);
        var items = await mockProductRepo.GetAllAsync();

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("Product not found.", exception.Message);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public async Task DeleteCancelledTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        DeleteProductHandler handler = new(mockProductRepo);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => handler.Handle(new DeleteProductCommand(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"), new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")), cts.Token);
        var items = await mockProductRepo.GetAllAsync();

        var exception = await Assert.ThrowsAsync<TaskCanceledException>(act);
        Assert.Equal("A task was canceled.", exception.Message);
        Assert.Equal(4, items.Count());
    }
}
