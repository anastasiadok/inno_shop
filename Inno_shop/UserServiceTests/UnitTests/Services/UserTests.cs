using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Domain.Exceptions;
using UserService.Infrastructure.Interfaces;
using UserServiceTests.UnitTests.Mocks;

namespace UserServiceTests.UnitTests.Services;

[Collection("UserUnit")]
public class UserTests
{
    private readonly IUserRepository mockUserRepo = MockUserRepository.GetUserRepository().Object;

    private readonly IUserService mockUserService;

    public UserTests()
    {
        mockUserService = new UServices.UserService(mockUserRepo);
    }

    [Fact]
    public void GetAllTest()
    {
        var result = mockUserService.GetAll().Result;

        Assert.IsType<List<UserDto>>(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public void GetByIdValidTest()
    {
        Guid idToGet = new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2");

        var result = mockUserService.GetById(idToGet).Result;

        Assert.IsType<UserDto>(result);
        Assert.Equal(idToGet, result.Id);
    }

    [Fact]
    public void GetByIdNotExistingTest()
    {
        var act = () => mockUserService.GetById(new("00000000-9ABD-4d9d-80C7-02AF85C822A2"));

        var exception = Assert.ThrowsAsync<NotFoundException>(act).Result;
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public void GetByEmailValidTest()
    {
        string emailToGet = "Adolf@gmail.com";

        var result = mockUserService.GetByEmail(emailToGet).Result;

        Assert.IsType<UserDto>(result);
        Assert.Equal(emailToGet, result.Email);
    }

    [Fact]
    public void GetByEmailNotExistingTest()
    {
        var act = () => mockUserService.GetByEmail("aaaaa@gmail.com");

        var exception = Assert.ThrowsAsync<NotFoundException>(act).Result;
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public void CreateValidTest()
    {
        UserRegisterDto userToAdd = new("user", "user@gmail.com", "userpassword");

        var result = mockUserService.Create(userToAdd).IsCompletedSuccessfully;
        var items = mockUserRepo.GetAllAsync().Result;

        Assert.True(result);
        Assert.Equal(4, items.Count());

        MockUserRepository.ResetData();
    }

    [Fact]
    public void CreateWithUsedEmailTest()
    {
        UserRegisterDto userToAdd = new("user", "Adolf@gmail.com", "userpassword");

        var act = () => mockUserService.Create(userToAdd);
        var items = mockUserRepo.GetAllAsync().Result;

        var exception = Assert.ThrowsAsync<BadRequestException>(act).Result;
        Assert.Equal("Email is already in use.", exception.Message);
        Assert.Equal(3, items.Count());
    }

    [Fact]
    public void UpdateValidTest()
    { 
        UserUpdateDto toUpdate = new(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"), "newname");

        var result = mockUserService.Update(toUpdate).IsCompletedSuccessfully;
        var item = mockUserRepo.GetByIdAsync(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")).Result;

        Assert.True(result);
        Assert.Equal("newname", item.Name);

        MockUserRepository.ResetData();
    }

    [Fact]
    public void UpdateNotExistingUserTest()
    {
        UserUpdateDto toUpdate = new(new("00000000-9ABD-4d9d-80C7-02AF85C822A2"), "newname");

        var act = () => mockUserService.Update(toUpdate);
        var items = mockUserRepo.GetAllAsync().Result;

        var exception = Assert.ThrowsAsync<NotFoundException>(act).Result;
        Assert.Equal("User not found.", exception.Message);
        Assert.Equal(3, items.Count());
    }

    [Fact]
    public void DeleteByIdValidTest()
    {
        var result = mockUserService.DeleteById(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")).IsCompletedSuccessfully;
        var items = mockUserRepo.GetAllAsync().Result;

        Assert.True(result);
        Assert.Equal(2, items.Count());

        MockUserRepository.ResetData();
    }

    [Fact]
    public void DeleteByIdNotExistingTest()
    {
        var act = () => mockUserService.DeleteById(new("00000000-9ABD-4d9d-80C7-02AF85C822A2"));

        var exception = Assert.ThrowsAsync<NotFoundException>(act).Result;
        Assert.Equal("User not found.", exception.Message);
    }
}
