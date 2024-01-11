using ProductService.Application.Dtos;
using ProductService.Application.ProductFeatures.Commands.UpdateProduct;
using ProductService.Domain.Exceptions;
using ProductServiceTests.UnitTests.Mocks;

namespace ProductServiceTests.UnitTests.Products.Commands;

public class UpdateHandlerTests
{

    [Fact]
    public async Task UpdateValidTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        UpdateProductHandler handler = new(mockProductRepo);

        UpdateProductDto toUpdate = new(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"), "John Doe Happy man", "ok.", 100, true);

        var result = handler.Handle(new UpdateProductCommand(toUpdate, new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None).IsCompletedSuccessfully;
        var item = await mockProductRepo.GetByIdAsync(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"));

        Assert.True(result);
        Assert.Equal("John Doe Happy man", item.Name);
        Assert.Equal("ok.", item.Description);
        Assert.Equal(100, item.Price);
        Assert.True(item.IsAvailible);
    }

    [Fact]
    public async Task UpdateNotExistingProductTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        UpdateProductHandler handler = new(mockProductRepo);

        UpdateProductDto toUpdate = new(new("00000000-9ABD-4d9d-80C7-02AF85C822A1"), "John Doe Happy man", "ok.", 100, true);

        var act = () => handler.Handle(new UpdateProductCommand(toUpdate, new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None);
        var items = await mockProductRepo.GetAllAsync();

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("Product not found.", exception.Message);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public async Task UpdateWithInvalidUserTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        UpdateProductHandler handler = new(mockProductRepo);

        UpdateProductDto toUpdate = new(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"), "John Doe Happy man", "ok.", 100, true);

        var act = () => handler.Handle(new UpdateProductCommand(toUpdate, new("00000000-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None);
        var items = await mockProductRepo.GetAllAsync();

        var exception = await Assert.ThrowsAsync<UserAccessException>(act);
        Assert.Equal("User can only manage their own products.", exception.Message);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public async Task UpdateCancelledTest()
    {
        var mockProductRepo = MockProductRepository.GetProductRepository().Object;
        UpdateProductHandler handler = new(mockProductRepo);

        UpdateProductDto toUpdate = new(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"), "John Doe Happy man", "ok.", 100, true);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => handler.Handle(new UpdateProductCommand(toUpdate, new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")), cts.Token);
        var items = await mockProductRepo.GetAllAsync();

        var exception = await Assert.ThrowsAsync<TaskCanceledException>(act);
        Assert.Equal("A task was canceled.", exception.Message);
        Assert.Equal(4, items.Count());
    }
}
