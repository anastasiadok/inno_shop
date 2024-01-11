using UserService.Domain.Entities;

namespace UserService.Infrastructure.Interfaces;

public interface IUserRepository
{
    Task<User> GetByIdAsync(Guid id);
    Task<User> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteByIdAsync(Guid id);
    Task<bool> CheckEmailIsFree(string email);
}
