using UserService.Application.Dtos;

namespace UserService.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetById(Guid id);
    Task<UserDto> GetByEmail(string email);
    IEnumerable<UserDto> GetAll();
    Task Update(UserUpdateDto userDto);
    Task DeleteById(Guid id);
    Task DeleteUserProducts(Guid id, string userJwt);
}
