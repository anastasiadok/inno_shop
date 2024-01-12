using ProductService.Application.ProductFeatures.Commands.DeleteProduct;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Interfaces;
using ProductServiceTests.UnitTests.Mocks;

namespace ProductServiceTests.UnitTests.Products.Commands;

[Collection("ProductUnit")]
public class DeleteHandlerTests
{
    private readonly IProductRepository mockProductRepo = MockProductRepository.GetProductRepository().Object;
    private readonly DeleteProductHandler handler;

    public DeleteHandlerTests() => handler = new(mockProductRepo);

    [Fact]
    public void DeleteValidTest()
    {
        var result = handler.Handle(new DeleteProductCommand(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"), new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None).IsCompletedSuccessfully;
        var items = mockProductRepo.GetAllAsync().Result;

        Assert.True(result);
        Assert.Equal(3, items.Count());

        MockProductRepository.ResetData();
    }

    [Fact]
    public void DeleteWithInvaidUserTest()
    {
        var act = () => handler.Handle(new DeleteProductCommand(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"), new("00000000-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None);
        var items = mockProductRepo.GetAllAsync().Result;

        var exception = Assert.ThrowsAsync<UserAccessException>(act).Result;
        Assert.Equal("User can only manage their own products.", exception.Message);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public void DeleteNotExistingTest()
    {
        var act = () => handler.Handle(new DeleteProductCommand(new("00000000-9ABD-4d9d-80C7-02AF85C822A1"), new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None);
        var items = mockProductRepo.GetAllAsync().Result;

        var exception = Assert.ThrowsAsync<NotFoundException>(act).Result;
        Assert.Equal("Product not found.", exception.Message);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public void DeleteCancelledTest()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => handler.Handle(new DeleteProductCommand(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"), new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")), cts.Token);
        var items = mockProductRepo.GetAllAsync().Result;

        var exception = Assert.ThrowsAsync<TaskCanceledException>(act).Result;
        Assert.Equal("A task was canceled.", exception.Message);
        Assert.Equal(4, items.Count());
    }
}
