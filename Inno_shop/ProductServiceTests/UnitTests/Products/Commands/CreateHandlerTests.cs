using ProductService.Application.Dtos;
using ProductService.Application.ProductFeatures.Commands.CreateProduct;
using ProductService.Domain.Exceptions;
using ProductServiceTests.UnitTests.Mocks;

namespace ProductServiceTests.UnitTests.Products.Commands;

public class CreateHandlerTests
{
    [Fact]
    public async Task AddValidTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        CreateProductHandler handler = new(mockProductRepo);

        CreateProductDto toAdd = new("John Doe Happy man 1", "ok.", 10, true, new("436DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var result = handler.Handle(new CreateProductCommand(toAdd, new("436DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None).IsCompletedSuccessfully;
        var items = await mockProductRepo.GetAllAsync();

        Assert.True(result);
        Assert.Equal(5, items.Count());
    }

    [Fact]
    public async Task AddWithInvaidUserTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        CreateProductHandler handler = new(mockProductRepo);

        CreateProductDto toAdd = new("John Doe Happy man 1", "ok.", 10, true, new("436DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var act = () => handler.Handle(new CreateProductCommand(toAdd, new("00000000-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None);
        var items = await mockProductRepo.GetAllAsync();

        var exception = await Assert.ThrowsAsync<UserAccessException>(act);
        Assert.Equal("User can only manage their own products.", exception.Message);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public async Task AddCancelledTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        CreateProductHandler handler = new(mockProductRepo);

        CreateProductDto toAdd = new("John Doe Happy man 1", "ok.", 10, true, new("436DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => handler.Handle(new CreateProductCommand(toAdd, new("436DA01F-9ABD-4d9d-80C7-02AF85C822A2")), cts.Token);
        var items = await mockProductRepo.GetAllAsync();

        var exception = await Assert.ThrowsAsync<TaskCanceledException>(act);
        Assert.Equal("A task was canceled.", exception.Message);
        Assert.Equal(4, items.Count());
    }
}
