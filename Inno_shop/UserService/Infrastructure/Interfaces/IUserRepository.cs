using UserService.Domain.Models;

namespace UserService.Infrastructure.Interfaces;

public interface IUserRepository
{
    Task<User> GetByIdAsync(Guid id);
    Task<User> GetByEmailAsync(string email);
    IEnumerable<User> GetAll();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteByIdAsync(Guid id);
}
