using Moq;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;

namespace ProductServiceTests.UnitTests.Mocks;

public static class MockProductRepository
{
    private static List<Product> list = new();

    public static void ResetData()
    {
        list = new()
        {
            new Product
            {
                Id = new("136DA01F-9ABD-4d9d-80C7-02AF85C822A1"),
                Name = "John Doe Happy man",
                CreationDate = DateTime.Now,
                CreatorId = new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"),
                Description = "ok.",
                Price = 10,
                IsAvailible = true
            },
            new Product
            {
                Id =new("236DA01F-9ABD-4d9d-80C7-02AF85C822A1"),
                Name = "Programming",
                CreationDate = DateTime.Now,
                CreatorId = new("236DA01F-9ABD-4d9d-80C7-02AF85C822A2"),
                Description = "do it 3 times a day and spina ne budet bolet",
                Price = 15,
                IsAvailible = true
            },
            new Product
            {
                Id =new("236DA01F-9ABD-4d9d-80C7-02AF85C822A3"),
                Name = "Programming1",
                CreationDate = DateTime.Now,
                CreatorId = new("236DA01F-9ABD-4d9d-80C7-02AF85C822A2"),
                Description = "do it 3 times a day and spina ne budet bolet",
                Price = 15,
                IsAvailible = true
            },
            new Product
            {
                Id =new("336DA01F-9ABD-4d9d-80C7-02AF85C822A1"),
                Name = "Bolshie Gorodaa",
                CreationDate = DateTime.Now,
                CreatorId = new("336DA01F-9ABD-4d9d-80C7-02AF85C822A2"),
                Description = "Pustie poezdaaa",
                Price = 10000,
                IsAvailible = false
            }
        };

    }
    public static Mock<IProductRepository> GetProductRepository()
    {
        ResetData();
        var mockRepo = new Mock<IProductRepository>();

        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

        mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>())).Returns((Product p) =>
        {
            list.Add(p);
            return Task.CompletedTask;
        });

        mockRepo.Setup(r => r.DeleteByIdAsync(It.IsAny<Guid>())).Returns((Guid id) =>
        {
            list.Remove(list.Find(p => p.Id == id));
            return Task.CompletedTask;
        });

        mockRepo.Setup(r => r.DeleteUserProductsAsync(It.IsAny<Guid>())).Returns((Guid id) =>
        {
            foreach (var pr in list.FindAll(p => p.CreatorId == id))
            {
                list.Remove(pr);
            }

            return Task.CompletedTask;
        });

        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns((Product pr) =>
        {
            list.Remove(list.Find(p => p.Id == pr.Id));
            list.Add(pr);

            return Task.CompletedTask;
        });

        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Guid id) => list.Find(p => p.Id == id));

        mockRepo.Setup(r => r.GetAllIQueryable()).Returns(list.AsQueryable());

        return mockRepo;
    }
}
