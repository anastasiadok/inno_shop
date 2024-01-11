using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using UserService;
using UserService.Application.Dtos;
using UserServiceTests.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using UserService.Infrastructure.Contexts;

namespace UserServiceTests.IntegrationTests;

public class TokenControllerTests: IClassFixture<CustomFactory<Program>>
{
    private readonly HttpClient _client;

    public TokenControllerTests(CustomFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task RefreshValidTest()
    {
        RefreshDto dto = new(await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"), "11111111-1111-1111-4444-555555555555");
        
        var request = new HttpRequestMessage(HttpMethod.Post, "api/tokens/refresh/");
        request.Content = JsonContent.Create(dto);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var responseDto = JsonSerializer.Deserialize<LoginResponseDto>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseDto.JwtToken);
    }

    [Fact]
    public async Task RefreshNotExistingTest()
    {
        RefreshDto dto = new(await JwtGenerator.GenerateJwt("johgggggggggggn@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"), "11111111-1111-1111-4444-555555555555");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/tokens/refresh/");
        request.Content = JsonContent.Create(dto);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public async Task RefreshInvalidRefreshTokenTest()
    {
        RefreshDto dto = new(await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"), "11111111-0000-1111-4444-555555555555");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/tokens/refresh/");
        request.Content = JsonContent.Create(dto);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Invalid refresh token.", details.Detail);
    }

    [Fact]
    public async Task RevokehValidTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/tokens/revoke/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        SeedData.PopulateTestData(appContext);
    }

    [Fact]
    public async Task RevokeNotExistingTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/tokens/revoke/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("johhhhn@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public async Task RevokeUnauthorizedTest()
    { 
        var request = new HttpRequestMessage(HttpMethod.Post, "api/tokens/revoke/");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
