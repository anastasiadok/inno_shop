using Mapster;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Domain.Exceptions;
using UserService.Domain.Models;
using UserService.Infrastructure.Interfaces;

namespace UserService.Application.Services;

public class UserService:IUserService
{
    private readonly IUserRepository _repository;

    private readonly IHttpClientFactory _httpClientFactory;

    public UserService(IUserRepository repository, IHttpClientFactory httpClientFactory) 
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
    }

    public IEnumerable<UserDto> GetAll()
    {
        var users = _repository.GetAll();
        return users.Adapt<IEnumerable<UserDto>>();
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

    public async Task DeleteUserProducts(Guid id, string userJwt)
    {
        if (userJwt is null)
            throw new UnauthorizedException();

        HttpClient httpClient = new();
        bool isDockerRun = Environment.GetEnvironmentVariable("MODE") == "container";
        int port = isDockerRun ? 5000 : 44371;
        string scheme = isDockerRun ? "http" : "https";

        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, $"{scheme}://localhost:{port}/api/products/user?userid=" + id);
        httpRequestMessage.Headers.Add("accept", "*/*");
        httpRequestMessage.Headers.Add("Authorization", userJwt);

        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
        var code = httpResponseMessage.StatusCode;
        if (!httpResponseMessage.IsSuccessStatusCode)
            throw new Exception($"Removal of user's products failed. Status {code}");
    } 
}
