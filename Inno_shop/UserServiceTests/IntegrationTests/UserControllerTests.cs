using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Net;
using UserServiceTests.IntegrationTests.Helpers;
using UserService;
using System.Text.Json;
using UserService.Application.Dtos;
using UserService.Infrastructure.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace UserServiceTests.IntegrationTests;

public class UserControllerTests : IClassFixture<CustomFactory<Program>>
{
    private HttpClient _client;

    public UserControllerTests(CustomFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task GetAllValidTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<UserDto>>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllUnauthorizedTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")]
    public async Task GetByIdValidTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UserDto>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Guid.Parse(userId), result.Id);
    }

    [Theory]
    [InlineData("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")]
    public async Task GetByIdUnauthorizedTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("236DA01F-0000-4d9d-80C7-02AF85C822A3")]
    public async Task GetByIdNotExsistingTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public async Task CreateValidTest()
    {
        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        SeedData.PopulateTestData(appContext);

        var user = new UserRegisterDto("pfjggfjfjjr", "chghgchc@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/users/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        request.Content = JsonContent.Create(user);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users/");
        getRequest.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);

        var getResponse = await _client.SendAsync(getRequest);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();
        var getResult = JsonSerializer.Deserialize<List<UserDto>>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(4, getResult.Count);
        
        SeedData.PopulateTestData(appContext);
    }

    [Fact]
    public async Task CreateUnauthorizedTest()
    {
        var user = new UserRegisterDto("pfjggfjfjjr", "chghgchc@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/users/");
        request.Content = JsonContent.Create(user);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateUsedEmailTest()
    {
        var user = new UserRegisterDto("pfjggfjfjjr", "john@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/users/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        request.Content = JsonContent.Create(user);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Email is already in use.", details.Detail);
    }

    [Fact]
    public async Task CreateValidationErrorTest()
    {
        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        SeedData.PopulateTestData(appContext);

        var user = new UserRegisterDto("pfjggfjfjjr", "il.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/users/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        request.Content = JsonContent.Create(user);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Email\":[\"'Email' is not a valid email address.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public async Task UpdateValidTest()
    {
        var user = new UserUpdateDto(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"), "pfjggfjfjjr");

        var request = new HttpRequestMessage(HttpMethod.Put, "api/users/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        request.Content = JsonContent.Create(user);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users/136DA01F-9ABD-4d9d-80C7-02AF85C822A2");
        getRequest.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);

        var getResponse = await _client.SendAsync(getRequest);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();
        var getResult = JsonSerializer.Deserialize<UserDto>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("pfjggfjfjjr", getResult.Name);
        
        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        SeedData.PopulateTestData(appContext);
    }

    [Fact]
    public async Task UpdateUnauthorizedTest()
    {
        var user = new UserUpdateDto(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"), "pfjggfjfjjr");

        var request = new HttpRequestMessage(HttpMethod.Put, "api/users/");
        request.Content = JsonContent.Create(user);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNotExsistingTest()
    {
        var user = new UserUpdateDto(new("136DA01F-0000-4d9d-80C7-02AF85C822A2"), "pfjggfjfjjr");

        var request = new HttpRequestMessage(HttpMethod.Put, "api/users/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        request.Content = JsonContent.Create(user);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public async Task UpdateValidationErrorTest()
    {
        var user = new UserUpdateDto(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"), "");

        var request = new HttpRequestMessage(HttpMethod.Put, "api/users/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        request.Content = JsonContent.Create(user);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Name\":[\"The length of 'Name' must be at least 4 characters. You entered 0 characters.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Theory]
    [InlineData("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")]
    public async Task DeleteByIdValidTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{userId}");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users/");
        getRequest.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);

        var getResponse = await _client.SendAsync(getRequest);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();
        var getResult = JsonSerializer.Deserialize<List<UserDto>>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(getResult.Where(p => p.Id == Guid.Parse(userId)));

        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        SeedData.PopulateTestData(appContext);
    }

    [Theory]
    [InlineData("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")]
    public async Task DeleteByIdUnauthorizedTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{userId}");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("136DA01F-0000-4d9d-80C7-02AF85C822A2")]
    public async Task DeleteByIdNotExsistingTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{userId}");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }
}