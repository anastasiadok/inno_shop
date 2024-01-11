using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Interfaces;

public interface IProductRepository
{
    Task<Product> GetByIdAsync(Guid id);
    Task<IEnumerable<Product>> GetAllAsync();
    IQueryable<Product> GetAllIQueryable();
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteByIdAsync(Guid id);
    Task DeleteUserProductsAsync(Guid userId);
}
