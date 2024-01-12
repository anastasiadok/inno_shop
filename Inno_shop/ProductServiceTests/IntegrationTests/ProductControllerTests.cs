using Microsoft.AspNetCore.Mvc;
using ProductService;
using ProductService.Application.Dtos;
using ProductService.Domain.Entities;
using ProductServiceTests.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProductServiceTests.IntegrationTests;

public class ProductControllerTests : IClassFixture<CustomFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductControllerTests(CustomFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public void GetAllValidTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/products/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<List<Product>>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetAllUnauthorizedTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/products/");

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("236DA01F-9ABD-4d9d-80C7-02AF85C822A3")]
    public void GetByIdValidTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{productId}");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<Product>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Guid.Parse(productId), result.Id);
    }

    [Theory]
    [InlineData("236DA01F-9ABD-4d9d-80C7-02AF85C822A3")]
    public void GetByIdUnauthorizedTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{productId}");

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("236DA01F-0000-4d9d-80C7-02AF85C822A3")]
    public void GetByIdNotExsistingTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{productId}");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Product not found.", details.Detail);
    }

    [Fact]
    public void GetFilteredSortedValidTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/products/filtersort?Filters=Price>20");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<List<Product>>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(result);
    }

    [Fact]
    public void GetFilteredSortedUnauthorizedTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/products/filtersort?Filters=Price>20");

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void GetFilteredSortedInvalidParamsTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/products/filtersort?Sorts=Price&PageSize=3&Page=-1");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void CreateValidTest()
    {
        var product = new CreateProductDto("pfjggfjfjjr", "hgvkbkhb", 100, true, new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var request = new HttpRequestMessage(HttpMethod.Post, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);
        request.Content = JsonContent.Create(product);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/products/");
        getRequest.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;

        var getResponse = _client.SendAsync(getRequest).Result;
        var getResponseString = getResponse.Content.ReadAsStringAsync().Result;
        var getResult = JsonSerializer.Deserialize<List<Product>>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, getResult.Count);

        SeedData.ResetData();
    }

    [Fact]
    public void CreateUnauthorizedTest()
    {
        var product = new CreateProductDto("pfjggfjfjjr", "hgvkbkhb", 100, true, new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var request = new HttpRequestMessage(HttpMethod.Post, "api/products/");
        request.Content = JsonContent.Create(product);

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void CreateValidationErrorTest()
    {
        var product = new CreateProductDto("pfjggfjfjjr", "hgvkbkhb", -100, true, new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"));

        var request = new HttpRequestMessage(HttpMethod.Post, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);
        request.Content = JsonContent.Create(product);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Price\":[\"'Price' must be greater than '0'.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public void CreateInvalidUserTest()
    {
        var product = new CreateProductDto("pfjggfjfjjr", "hgvkbkhb", 100, true, new("136DA01F-0000-4d9d-80C7-02AF85C822A2"));

        var request = new HttpRequestMessage(HttpMethod.Post, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);
        request.Content = JsonContent.Create(product);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("User can only manage their own products.", details.Detail);
    }

    [Fact]
    public void UpdateValidTest()
    {
        var product = new UpdateProductDto(new("236DA01F-9ABD-4d9d-80C7-02AF85C822A3"), "pfjggfjfjjr", "hgvkbkhb", 100, true);

        var request = new HttpRequestMessage(HttpMethod.Put, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);
        request.Content = JsonContent.Create(product);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/products/236DA01F-9ABD-4d9d-80C7-02AF85C822A3");
        getRequest.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;

        var getResponse = _client.SendAsync(getRequest).Result;
        var getResponseString = getResponse.Content.ReadAsStringAsync().Result;
        var getResult = JsonSerializer.Deserialize<Product>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("pfjggfjfjjr", getResult.Name);

        SeedData.ResetData();
    }

    [Fact]
    public void UpdateUnauthorizedTest()
    {
        var product = new UpdateProductDto(new("236DA01F-9ABD-4d9d-80C7-02AF85C822A3"), "pfjggfjfjjr", "hgvkbkhb", 100, true);

        var request = new HttpRequestMessage(HttpMethod.Put, "api/products/");
        request.Content = JsonContent.Create(product);

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void UpdateNotExsistingTest()
    {
        var product = new UpdateProductDto(new("236DA01F-0000-4d9d-80C7-02AF85C822A3"), "pfjggfjfjjr", "hgvkbkhb", 100, true);

        var request = new HttpRequestMessage(HttpMethod.Put, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);
        request.Content = JsonContent.Create(product);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Product not found.", details.Detail);
    }

    [Fact]
    public void UpdateValidationErrorTest()
    {
        var product = new UpdateProductDto(new("236DA01F-9ABD-4d9d-80C7-02AF85C822A3"), "pfjggfjfjjr", "hgvkbkhb", -100, true);

        var request = new HttpRequestMessage(HttpMethod.Put, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);
        request.Content = JsonContent.Create(product);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Price\":[\"'Price' must be greater than '0'.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public void UpdateInvalidUserTest()
    {
        var product = new UpdateProductDto(new("236DA01F-9ABD-4d9d-80C7-02AF85C822A3"), "pfjggfjfjjr", "hgvkbkhb", 100, true);

        var request = new HttpRequestMessage(HttpMethod.Put, "api/products/");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-0000-4d9d-80C7-02AF85C822A2").Result);
        request.Content = JsonContent.Create(product);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("User can only manage their own products.", details.Detail);
    }

    [Theory]
    [InlineData("236DA01F-9ABD-4d9d-80C7-02AF85C822A3")]
    public void DeleteByIdValidTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/products/{productId}");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/products/");
        getRequest.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;

        var getResponse = _client.SendAsync(getRequest).Result;
        var getResponseString = getResponse.Content.ReadAsStringAsync().Result;
        var getResult = JsonSerializer.Deserialize<List<Product>>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(getResult.Where(p => p.Id == Guid.Parse(productId)));

        SeedData.ResetData();
    }

    [Theory]
    [InlineData("236DA01F-9ABD-4d9d-80C7-02AF85C822A3")]
    public void DeleteByIdUnauthorizedTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/products/{productId}");

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("236DA01F-0000-4d9d-80C7-02AF85C822A3")]
    public void DeleteByIdNotExsistingTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/products/{productId}");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Product not found.", details.Detail);
    }

    [Theory]
    [InlineData("236DA01F-9ABD-4d9d-80C7-02AF85C822A3")]
    public void DeleteByIdInvalidUserTest(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/products/{productId}");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-0000-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("User can only manage their own products.", details.Detail);
    }

    [Theory]
    [InlineData("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")]
    public void DeleteUserProductsValidTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/products/user/{userId}");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/products/");
        getRequest.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;

        var getResponse = _client.SendAsync(getRequest).Result;
        var getResponseString = getResponse.Content.ReadAsStringAsync().Result;
        var getResult = JsonSerializer.Deserialize<List<Product>>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(getResult.Where(p => p.CreatorId == Guid.Parse(userId)));

        SeedData.ResetData();
    }

    [Theory]
    [InlineData("136DA01F-9ABD-4d9d-80C7-02AF85C822A2")]
    public void DeleteUserProductsUnauthorizedTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/products/user/{userId}");

        var response = _client.SendAsync(request).Result;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("136DA01F-0000-4d9d-80C7-02AF85C822A2")]
    public void DeleteUserProductsInvalidUserTest(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/products/user/{userId}");
        request.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("User can only manage their own products.", details.Detail);
    }
}
