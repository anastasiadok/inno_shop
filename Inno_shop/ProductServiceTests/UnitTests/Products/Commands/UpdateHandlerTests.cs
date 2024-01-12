using ProductService.Application.Dtos;
using ProductService.Application.ProductFeatures.Commands.UpdateProduct;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Interfaces;
using ProductServiceTests.UnitTests.Mocks;

namespace ProductServiceTests.UnitTests.Products.Commands;

[Collection("ProductUnit")]
public class UpdateHandlerTests
{
    private readonly IProductRepository mockProductRepo = MockProductRepository.GetProductRepository().Object;
    private readonly UpdateProductHandler handler;

    public UpdateHandlerTests() => handler = new(mockProductRepo);


    [Fact]
    public void UpdateValidTest()
    {
        UpdateProductDto toUpdate = new(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"), "John Doe Happy man", "ok.", 100, true);

        var result = handler.Handle(new UpdateProductCommand(toUpdate, new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None).IsCompletedSuccessfully;
        var item = mockProductRepo.GetByIdAsync(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1")).Result;

        Assert.True(result);
        Assert.Equal("John Doe Happy man", item.Name);
        Assert.Equal("ok.", item.Description);
        Assert.Equal(100, item.Price);
        Assert.True(item.IsAvailible);

        MockProductRepository.ResetData();
    }

    [Fact]
    public void UpdateNotExistingProductTest()
    {
        UpdateProductDto toUpdate = new(new("00000000-9ABD-4d9d-80C7-02AF85C822A1"), "John Doe Happy man", "ok.", 100, true);

        var act = () => handler.Handle(new UpdateProductCommand(toUpdate, new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None);
        var items = mockProductRepo.GetAllAsync().Result;

        var exception = Assert.ThrowsAsync<NotFoundException>(act).Result;
        Assert.Equal("Product not found.", exception.Message);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public void UpdateWithInvalidUserTest()
    {
        UpdateProductDto toUpdate = new(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"), "John Doe Happy man", "ok.", 100, true);

        var act = () => handler.Handle(new UpdateProductCommand(toUpdate, new("00000000-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None);
        var items = mockProductRepo.GetAllAsync().Result;

        var exception = Assert.ThrowsAsync<UserAccessException>(act).Result;
        Assert.Equal("User can only manage their own products.", exception.Message);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public void UpdateCancelledTest()
    {
        UpdateProductDto toUpdate = new(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"), "John Doe Happy man", "ok.", 100, true);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => handler.Handle(new UpdateProductCommand(toUpdate, new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")), cts.Token);
        var items = mockProductRepo.GetAllAsync().Result;

        var exception = Assert.ThrowsAsync<TaskCanceledException>(act).Result;
        Assert.Equal("A task was canceled.", exception.Message);
        Assert.Equal(4, items.Count());
    }
}
