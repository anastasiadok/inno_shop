using Mapster;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Domain.Exceptions;
using UserService.Domain.Entities;
using UserService.Infrastructure.Interfaces;

namespace UserService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UserDto>> GetAll()
    {
        var users = await _repository.GetAllAsync();
        return users.Adapt<List<UserDto>>();
    }

    public async Task<UserDto> GetByEmail(string email)
    {
        var user = await _repository.GetByEmailAsync(email) ?? throw new NotFoundException(nameof(User));
        return user.Adapt<UserDto>();
    }

    public async Task<UserDto> GetById(Guid id)
    {
        var user = await _repository.GetByIdAsync(id) ?? throw new NotFoundException(nameof(User));
        return user.Adapt<UserDto>();
    }

    public async Task Create(UserRegisterDto userDto)
    {
        var emailUser = await _repository.GetByEmailAsync(userDto.Email);
        if (emailUser is not null)
            throw new BadRequestException("Email is already in use.");

        var user = userDto.Adapt<User>();
        AuthService.CreatePasswordHashAndSalt(userDto.Password, out byte[] hash, out byte[] salt);

        user.Id = Guid.NewGuid();
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.IsEmailConfirmed = true;

        await _repository.AddAsync(user);
    }
    public async Task Update(UserUpdateDto userDto)
    {
        var user = await _repository.GetByIdAsync(userDto.Id) ?? throw new NotFoundException(nameof(User));
        userDto.Adapt(user);
        await _repository.UpdateAsync(user);
    }

    public async Task DeleteById(Guid id)
    {
        var user = await _repository.GetByIdAsync(id) ?? throw new NotFoundException(nameof(User));
        await _repository.DeleteByIdAsync(id);
    }
}