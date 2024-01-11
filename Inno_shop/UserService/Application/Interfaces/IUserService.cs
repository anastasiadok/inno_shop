using UserService.Application.Dtos;

namespace UserService.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetById(Guid id);
    Task<UserDto> GetByEmail(string email);
    Task<IEnumerable<UserDto>> GetAll();
    Task Create(UserRegisterDto userDto);
    Task Update(UserUpdateDto userDto);
    Task DeleteById(Guid id);
}
