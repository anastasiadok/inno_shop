using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ProductService;
using ProductService.Application.Dtos;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Contexts;
using ProductServiceTests.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProductServiceTests.IntegrationTests;

public class ProductControllerTests: IClassFixture<CustomFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductControllerTests(CustomFactory<Program> factory)
            => _client = factory.CreateClient();

    [Fact]
    public async Task GetAllValidTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/products/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com","136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        
        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Product>>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllUnauthorizedTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/products/");
        
        var response = await _client.SendAsync(request);
       
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("236DA01F-9ABD-4d9d-80C7-02AF85C822A3")]
    public async Task GetByIdValidTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{productId}");
        request.Headers.Add("Authorization", "Bearer " +await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Product>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Guid.Parse(productId), result.Id);
    }

    [Theory]
    [InlineData("236DA01F-9ABD-4d9d-80C7-02AF85C822A3")]
    public async Task GetByIdUnauthorizedTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{productId}");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("236DA01F-0000-4d9d-80C7-02AF85C822A3")]
    public async Task GetByIdNotExsistingTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{productId}");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details  = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Product not found.",details.Detail);
    }

    [Fact]
    public async Task GetFilteredSortedValidTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/products/filtersort?Filters=Price>20");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Product>>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1,result.Count);
    }

    [Fact]
    public async Task GetFilteredSortedUnauthorizedTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/products/filtersort?Filters=Price>20");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetFilteredSortedInvalidParamsTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/products/filtersort?Sorts=Price&PageSize=3&Page=-1");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateValidTest()
    {
        var product = new CreateProductDto("pfjggfjfjjr", "hgvkbkhb", 100, true, new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var request = new HttpRequestMessage(HttpMethod.Post, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        request.Content = JsonContent.Create(product);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/products/");
        getRequest.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);

        var getResponse = await _client.SendAsync(getRequest);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();
        var getResult = JsonSerializer.Deserialize<List<Product>>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, getResult.Count);

        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        SeedData.PopulateTestData(appContext);
    }

    [Fact]
    public async Task CreateUnauthorizedTest()
    {
        var product = new CreateProductDto("pfjggfjfjjr", "hgvkbkhb", 100, true, new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var request = new HttpRequestMessage(HttpMethod.Post, "api/products/");
        request.Content = JsonContent.Create(product);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateValidationErrorTest()
    {
        var product = new CreateProductDto("pfjggfjfjjr", "hgvkbkhb", -100, true, new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var request = new HttpRequestMessage(HttpMethod.Post, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        request.Content = JsonContent.Create(product);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Price\":[\"'Price' must be greater than '0'.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public async Task CreateInvalidUserTest()
    {
        var product = new CreateProductDto("pfjggfjfjjr", "hgvkbkhb", 100, true, new("136DA01F-0000-4d9d-80C7-02AF85C822A2"));

        var request = new HttpRequestMessage(HttpMethod.Post, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        request.Content = JsonContent.Create(product);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("User can only manage their own products.", details.Detail);
    }

    [Fact]
    public async Task UpdateValidTest()
    {
        var product = new UpdateProductDto(new("236DA01F-9ABD-4d9d-80C7-02AF85C822A3"), "pfjggfjfjjr", "hgvkbkhb", 100, true);

        var request = new HttpRequestMessage(HttpMethod.Put, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        request.Content = JsonContent.Create(product);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/products/236DA01F-9ABD-4d9d-80C7-02AF85C822A3");
        getRequest.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);

        var getResponse = await _client.SendAsync(getRequest);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();
        var getResult = JsonSerializer.Deserialize<Product>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("pfjggfjfjjr", getResult.Name);

        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        SeedData.PopulateTestData(appContext);
    }

    [Fact]
    public async Task UpdateUnauthorizedTest()
    {
        var product = new UpdateProductDto(new("236DA01F-9ABD-4d9d-80C7-02AF85C822A3"), "pfjggfjfjjr", "hgvkbkhb", 100, true);

        var request = new HttpRequestMessage(HttpMethod.Put, "api/products/");
        request.Content = JsonContent.Create(product);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNotExsistingTest()
    {
        var product = new UpdateProductDto(new("236DA01F-0000-4d9d-80C7-02AF85C822A3"), "pfjggfjfjjr", "hgvkbkhb", 100, true);

        var request = new HttpRequestMessage(HttpMethod.Put, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        request.Content = JsonContent.Create(product);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Product not found.", details.Detail);
    }

    [Fact]
    public async Task UpdateValidationErrorTest()
    {
        var product = new UpdateProductDto(new("236DA01F-9ABD-4d9d-80C7-02AF85C822A3"), "pfjggfjfjjr", "hgvkbkhb", -100, true);

        var request = new HttpRequestMessage(HttpMethod.Put, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
        request.Content = JsonContent.Create(product);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Price\":[\"'Price' must be greater than '0'.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public async Task UpdateInvalidUserTest()
    {
        var product = new UpdateProductDto(new("236DA01F-9ABD-4d9d-80C7-02AF85C822A3"), "pfjggfjfjjr", "hgvkbkhb", 100, true);

        var request = new HttpRequestMessage(HttpMethod.Put, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-0000-4d9d-80C7-02AF85C822A2"));
        request.Content = JsonContent.Create(product);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("User can only manage their own products.", details.Detail);
    }

    [Theory]
    [InlineData("236DA01F-9ABD-4d9d-80C7-02AF85C822A3")]
    public async Task DeleteByIdValidTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/products/{productId}");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/products/");
        getRequest.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);

        var getResponse = await _client.SendAsync(getRequest);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();
        var getResult = JsonSerializer.Deserialize<List<Product>>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(getResult.Where(p=>p.Id==Guid.Parse(productId)));

        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        SeedData.PopulateTestData(appContext);
    }

    [Theory]
    [InlineData("236DA01F-9ABD-4d9d-80C7-02AF85C822A3")]
    public async Task DeleteByIdUnauthorizedTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/products/{productId}");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("236DA01F-0000-4d9d-80C7-02AF85C822A3")]
    public async Task DeleteByIdNotExsistingTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/products/{productId}");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Product not found.", details.Detail);
    }

    [Theory]
    [InlineData("236DA01F-9ABD-4d9d-80C7-02AF85C822A3")]
    public async Task DeleteByIdInvalidUserTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/products/{productId}");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-0000-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("User can only manage their own products.", details.Detail);
    }

    [Theory]
    [InlineData("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")]
    public async Task DeleteUserProductsValidTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/products/user/{userId}");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));
       
        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/products/");
        getRequest.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);

        var getResponse = await _client.SendAsync(getRequest);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();
        var getResult = JsonSerializer.Deserialize<List<Product>>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(getResult.Where(p=>p.CreatorId==Guid.Parse(userId)));

        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        SeedData.PopulateTestData(appContext);
    }

    [Theory]
    [InlineData("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")]
    public async Task DeleteUserProductsUnauthorizedTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/products/user/{userId}");
        
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("136DA01F-0000-4d9d-80C7-02AF85C822A2")]
    public async Task DeleteUserProductsInvalidUserTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/products/user/{userId}");
        request.Headers.Add("Authorization", "Bearer " + await JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("User can only manage their own products.", details.Detail);
    }
}
