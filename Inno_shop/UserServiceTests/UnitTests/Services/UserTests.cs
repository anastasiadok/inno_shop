using UserService.Application.Dtos;
using UserService.Domain.Exceptions;
using UserServiceTests.UnitTests.Mocks;

namespace UserServiceTests.UnitTests.Services;

public class UserTests
{
    [Fact]
    public async Task GetAllTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        UServices.UserService service = new(mockUserRepo);

        var result = await service.GetAll();

        Assert.IsType<List<UserDto>>(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetByIdValidTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        UServices.UserService service = new(mockUserRepo);

        Guid idToGet = new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2");

        var result = await service.GetById(idToGet);

        Assert.IsType<UserDto>(result);
        Assert.Equal(idToGet, result.Id);
    }

    [Fact]
    public async Task GetByIdNotExistingTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        UServices.UserService service = new(mockUserRepo);

        var act = () => service.GetById(new("00000000-9ABD-4d9d-80C7-02AF85C822A2"));

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public async Task GetByEmailValidTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        UServices.UserService service = new(mockUserRepo);

        string emailToGet = "Adolf@gmail.com";

        var result = await service.GetByEmail(emailToGet);

        Assert.IsType<UserDto>(result);
        Assert.Equal(emailToGet, result.Email);
    }

    [Fact]
    public async Task GetByEmailNotExistingTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        UServices.UserService service = new(mockUserRepo);

        var act = () => service.GetByEmail("aaaaa@gmail.com");

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public async Task CreateValidTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        UServices.UserService service = new(mockUserRepo);

        UserRegisterDto userToAdd = new("user", "user@gmail.com", "userpassword");

        var result = service.Create(userToAdd).IsCompletedSuccessfully;
        var items = await mockUserRepo.GetAllAsync();

        Assert.True(result);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public async Task CreateWithUsedEmailTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        UServices.UserService service = new(mockUserRepo);

        UserRegisterDto userToAdd = new("user", "Adolf@gmail.com", "userpassword");

        var act = () => service.Create(userToAdd);
        var items = await mockUserRepo.GetAllAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("Email is already in use.", exception.Message);
        Assert.Equal(3, items.Count());
    }

    [Fact]
    public async Task UpdateValidTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        UServices.UserService service = new(mockUserRepo);

        UserUpdateDto toUpdate = new(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"), "newname");

        var result = service.Update(toUpdate).IsCompletedSuccessfully;
        var item = await mockUserRepo.GetByIdAsync(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        Assert.True(result);
        Assert.Equal("newname", item.Name);
    }

    [Fact]
    public async Task UpdateNotExistingUserTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        UServices.UserService service = new(mockUserRepo);

        UserUpdateDto toUpdate = new(new("00000000-9ABD-4d9d-80C7-02AF85C822A2"), "newname");

        var act = () => service.Update(toUpdate);
        var items = await mockUserRepo.GetAllAsync();

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("User not found.", exception.Message);
        Assert.Equal(3, items.Count());
    }

    [Fact]
    public async Task DeleteByIdValidTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        UServices.UserService service = new(mockUserRepo);

        var result = service.DeleteById(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")).IsCompletedSuccessfully;
        var items = await mockUserRepo.GetAllAsync();

        Assert.True(result);
        Assert.Equal(2, items.Count());
    }

    [Fact]
    public async Task DeleteByIdNotExistingTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        UServices.UserService service = new(mockUserRepo);

        var act = () => service.DeleteById(new("00000000-9ABD-4d9d-80C7-02AF85C822A2"));

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("User not found.", exception.Message);
    }
}
