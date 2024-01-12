using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Net;
using UserServiceTests.IntegrationTests.Helpers;
using UserService;
using System.Text.Json;
using UserService.Application.Dtos;

namespace UserServiceTests.IntegrationTests;

[Collection("UserIntegration")]
public class UserControllerTests : IClassFixture<CustomFactory<Program>>
{
    private readonly HttpClient _client;

    public UserControllerTests(CustomFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public void GetAllValidTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<List<UserDto>>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void GetAllUnauthorizedTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/");

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")]
    public void GetByIdValidTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<UserDto>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Guid.Parse(userId), result.Id);
    }

    [Theory]
    [InlineData("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")]
    public void GetByIdUnauthorizedTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("236DA01F-0000-4d9d-80C7-02AF85C822A3")]
    public void GetByIdNotExsistingTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public void CreateValidTest()
    {
        var user = new UserRegisterDto("pfjggfjfjjr", "chghgchc@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/users/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);
        request.Content = JsonContent.Create(user);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users/");
        getRequest.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;

        var getResponse = _client.SendAsync(getRequest).Result;
        var getResponseString = getResponse.Content.ReadAsStringAsync().Result;
        var getResult = JsonSerializer.Deserialize<List<UserDto>>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(4, getResult.Count);

        SeedData.ResetData();
    }

    [Fact]
    public void CreateUnauthorizedTest()
    {
        var user = new UserRegisterDto("pfjggfjfjjr", "chghgchc@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/users/");
        request.Content = JsonContent.Create(user);

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        SeedData.ResetData();
    }

    [Fact]
    public void CreateUsedEmailTest()
    {
        var user = new UserRegisterDto("pfjggfjfjjr", "john@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/users/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);
        request.Content = JsonContent.Create(user);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Email is already in use.", details.Detail);
    }

    [Fact]
    public void CreateValidationErrorTest()
    {
        var user = new UserRegisterDto("pfjggfjfjjr", "il.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/users/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);
        request.Content = JsonContent.Create(user);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Email\":[\"'Email' is not a valid email address.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public void UpdateValidTest()
    {
        var user = new UserUpdateDto(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"), "pfjggfjfjjr");

        var request = new HttpRequestMessage(HttpMethod.Put, "api/users/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);
        request.Content = JsonContent.Create(user);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users/136DA01F-9ABD-4d9d-80C7-02AF85C822A2");
        getRequest.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;

        var getResponse = _client.SendAsync(getRequest).Result;
        var getResponseString = getResponse.Content.ReadAsStringAsync().Result;
        var getResult = JsonSerializer.Deserialize<UserDto>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("pfjggfjfjjr", getResult.Name);

        SeedData.ResetData();
    }

    [Fact]
    public void UpdateUnauthorizedTest()
    {
        var user = new UserUpdateDto(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"), "pfjggfjfjjr");

        var request = new HttpRequestMessage(HttpMethod.Put, "api/users/");
        request.Content = JsonContent.Create(user);

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void UpdateNotExsistingTest()
    {
        var user = new UserUpdateDto(new("136DA01F-0000-4d9d-80C7-02AF85C822A2"), "pfjggfjfjjr");

        var request = new HttpRequestMessage(HttpMethod.Put, "api/users/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);
        request.Content = JsonContent.Create(user);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public void UpdateValidationErrorTest()
    {
        var user = new UserUpdateDto(new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"), "");

        var request = new HttpRequestMessage(HttpMethod.Put, "api/users/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);
        request.Content = JsonContent.Create(user);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Name\":[\"The length of 'Name' must be at least 4 characters. You entered 0 characters.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Theory]
    [InlineData("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")]
    public void DeleteByIdValidTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{userId}");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users/");
        getRequest.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;

        var getResponse = _client.SendAsync(getRequest).Result;
        var getResponseString = getResponse.Content.ReadAsStringAsync().Result;
        var getResult = JsonSerializer.Deserialize<List<UserDto>>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(getResult.Where(p => p.Id == Guid.Parse(userId)));

        SeedData.ResetData();
    }

    [Theory]
    [InlineData("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")]
    public void DeleteByIdUnauthorizedTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{userId}");

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        SeedData.ResetData();
    }

    [Theory]
    [InlineData("136DA01F-0000-4d9d-80C7-02AF85C822A2")]
    public void DeleteByIdNotExsistingTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{userId}");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }
}