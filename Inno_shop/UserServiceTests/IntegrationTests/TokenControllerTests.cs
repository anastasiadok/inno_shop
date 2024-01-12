using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using UserService;
using UserService.Application.Dtos;
using UserServiceTests.IntegrationTests.Helpers;

namespace UserServiceTests.IntegrationTests;

[Collection("Sequential")]
public class TokenControllerTests : IClassFixture<CustomFactory<Program>>
{
    private readonly HttpClient _client;

    public TokenControllerTests(CustomFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public void RefreshValidTest()
    {
        RefreshDto dto = new(JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result, "11111111-1111-1111-4444-555555555555");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/tokens/refresh/");
        request.Content = JsonContent.Create(dto);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var responseDto = JsonSerializer.Deserialize<LoginResponseDto>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseDto.JwtToken);
    }

    [Fact]
    public void RefreshNotExistingTest()
    {
        RefreshDto dto = new(JwtGenerator.GenerateJwt("johgggggggggggn@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result, "11111111-1111-1111-4444-555555555555");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/tokens/refresh/");
        request.Content = JsonContent.Create(dto);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public void RefreshInvalidRefreshTokenTest()
    {
        RefreshDto dto = new(JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result, "11111111-0000-1111-4444-555555555555");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/tokens/refresh/");
        request.Content = JsonContent.Create(dto);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Invalid refresh token.", details.Detail);
    }

    [Fact]
    public void RevokehValidTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/tokens/revoke/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        SeedData.ResetData();
    }

    [Fact]
    public void RevokeNotExistingTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/tokens/revoke/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("johhhhn@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public void RevokeUnauthorizedTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/tokens/revoke/");

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
