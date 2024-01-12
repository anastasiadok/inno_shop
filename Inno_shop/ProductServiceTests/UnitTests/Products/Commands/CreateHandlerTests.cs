using ProductService.Application.Dtos;
using ProductService.Application.ProductFeatures.Commands.CreateProduct;
using ProductService.Domain.Exceptions;
using ProductService.Infrastructure.Interfaces;
using ProductServiceTests.UnitTests.Mocks;

namespace ProductServiceTests.UnitTests.Products.Commands;

[Collection("ProductUnit")]
public class CreateHandlerTests
{
    private readonly IProductRepository mockProductRepo = MockProductRepository.GetProductRepository().Object;
    private readonly CreateProductHandler handler;

    public CreateHandlerTests() => handler = new(mockProductRepo);

    [Fact]
    public void AddValidTest()
    {
        CreateProductDto toAdd = new("John Doe Happy man 1", "ok.", 10, true, new("436DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var result = handler.Handle(new CreateProductCommand(toAdd, new("436DA01F-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None).IsCompletedSuccessfully;
        var items = mockProductRepo.GetAllAsync().Result;

        Assert.True(result);
        Assert.Equal(5, items.Count());

        MockProductRepository.ResetData();
    }

    [Fact]
    public void AddWithInvaidUserTest()
    {
        CreateProductDto toAdd = new("John Doe Happy man 1", "ok.", 10, true, new("436DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var act = () => handler.Handle(new CreateProductCommand(toAdd, new("00000000-9ABD-4d9d-80C7-02AF85C822A2")), CancellationToken.None);
        var items = mockProductRepo.GetAllAsync().Result;

        var exception = Assert.ThrowsAsync<UserAccessException>(act).Result;
        Assert.Equal("User can only manage their own products.", exception.Message);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public void AddCancelledTest()
    {
        CreateProductDto toAdd = new("John Doe Happy man 1", "ok.", 10, true, new("436DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => handler.Handle(new CreateProductCommand(toAdd, new("436DA01F-9ABD-4d9d-80C7-02AF85C822A2")), cts.Token);
        var items = mockProductRepo.GetAllAsync().Result;

        var exception = Assert.ThrowsAsync<TaskCanceledException>(act).Result;
        Assert.Equal("A task was canceled.", exception.Message);
        Assert.Equal(4, items.Count());
    }
}
