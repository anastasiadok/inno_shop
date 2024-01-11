using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Net;
using UserService.Application.Dtos;
using UserServiceTests.IntegrationTests.Helpers;
using UserService;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using UserService.Infrastructure.Contexts;

namespace UserServiceTests.IntegrationTests;

public class AuthControllerTests : IClassFixture<CustomFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task RegisterUsedEmailTest()
    {
        var user = new UserRegisterDto("pfjggfjfjjr", "john@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/register/");
        request.Content = JsonContent.Create(user);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Email is already in use.", details.Detail);
    }

    [Fact]
    public async Task RegisterValidationErrorTest()
    {
        var user = new UserRegisterDto("pfjggfjfjjr", "chghgchc@gmail.com", "");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/register/");
        request.Content = JsonContent.Create(user);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Password\":[\"The length of 'Password' must be at least 8 characters. You entered 0 characters.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public async Task LoginValidTest()
    {
        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        SeedData.PopulateTestData(appContext);

        var user = new LoginDto("john@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login/");
        request.Content = JsonContent.Create(user);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var responseDto = JsonSerializer.Deserialize<LoginResponseDto>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseDto.JwtToken);

        SeedData.PopulateTestData(appContext);
    }

    [Fact]
    public async Task LoginNotExistingEmailTest()
    {
        var user = new LoginDto("fchgvjhb@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login/");
        request.Content = JsonContent.Create(user);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public async Task LoginInvalidPasswordTest()
    {
        var user = new LoginDto("john@gmail.com", "passsssssssssssword");
        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login/");
        request.Content = JsonContent.Create(user);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Invalid password.", details.Detail);
    }

    [Fact]
    public async Task LoginNotConfirmedEmailTest()
    {
        var user = new LoginDto("nameee@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login/");
        request.Content = JsonContent.Create(user);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Email is not confirmed.", details.Detail);
    }

    [Fact]
    public async Task LoginValidationErrorTest()
    {
        var user = new LoginDto("johfhgvjilcom", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login/");
        request.Content = JsonContent.Create(user);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Email\":[\"'Email' is not a valid email address.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public async Task ForgotPasswordValidTest()
    {
        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        SeedData.PopulateTestData(appContext);

        var email = "john@gmail.com";

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/auth/forgotpassword/?email={email}");
        request.Content = JsonContent.Create(email);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"Check {email} email. You may now reset your password whithin 1 hour.", responseString);

        SeedData.PopulateTestData(appContext);
    }

    [Fact]
    public async Task ForgotPasswordValidationErrorTest()
    {
        var email = "johfhgailcom";

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/auth/forgotpassword/?email={email}");
        request.Content = JsonContent.Create(email);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"email\":[\"The email field is not a valid e-mail address.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public async Task ForgotPasswordNotExistingTest()
    {
        var email = "joooooohn@gmail.com";

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/auth/forgotpassword/?email={email}");
        request.Content = JsonContent.Create(email);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public async Task ConfirmEmailValidTest()
    {
        var email = "nameee@gmail.com";
        var token = "11111111-2222-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/confirmemail?email={email}&token={token}");

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Confirmation was successful.", responseString);

        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        SeedData.PopulateTestData(appContext);
    }

    [Fact]
    public async Task ConfirmEmailNotExistingTest()
    {
        var email = "joooohn@gmail.com";
        var token = "11111111-2222-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/confirmemail?email={email}&token={token}");

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public async Task ConfirmConfirmedEmailTest()
    {
        var email = "john@gmail.com";
        var token = "11111111-2222-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/confirmemail?email={email}&token={token}");

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Email is already confirmed.", details.Detail);
    }

    [Fact]
    public async Task ConfirmEmailInvalidTokenTest()
    {
        var email = "nameee@gmail.com";
        var token = "11111111-0000-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/confirmemail?email={email}&token={token}");

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Invalid token.", details.Detail);
    }

    [Fact]
    public async Task ConfirmEmailValidationErrorTest()
    {
        var email = "namhgvjhbail.com";
        var token = "11111111-2222-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/confirmemail?email={email}&token={token}");

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"email\":[\"The email field is not a valid e-mail address.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public async Task GetResetPasswordValidTest()
    {
        var email = "nameee@gmail.com";
        var token = "11111111-2222-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/resetpassword?email={email}&token={token}");

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var resetDto = JsonSerializer.Deserialize<ResetPasswordDto>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(email, resetDto.Email);
    }

    [Fact]
    public async Task GetResetPasswordValidationErrorTest()
    {
        var email = "namhgvjhbail.com";
        var token = "11111111-2222-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/resetpassword?email={email}&token={token}");

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"email\":[\"The email field is not a valid e-mail address.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public async Task ResetPasswordPostValidTest()
    {
        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        SeedData.PopulateTestData(appContext);

        ResetPasswordDto resetDto = new("john@gmail.com", "newpassword", "11111111-1111-1111-1111-111111111111");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/resetpassword/");
        request.Content = JsonContent.Create(resetDto);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Password has been successfully changed.", responseString);

        SeedData.PopulateTestData(appContext);
    }

    [Fact]
    public async Task ResetPasswordPostNotExistingTest()
    {
        ResetPasswordDto resetDto = new("johhhhhhn@gmail.com", "newpassword", "11111111-2222-3333-4444-555555555555");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/resetpassword/");
        request.Content = JsonContent.Create(resetDto);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public async Task ResetPasswordInvalidTokenTest()
    {
        ResetPasswordDto resetDto = new("john@gmail.com", "newpassword", "11111111-0000-3333-4444-555555555555");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/resetpassword/");
        request.Content = JsonContent.Create(resetDto);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Invalid token.", details.Detail);
    }

    [Fact]
    public async Task ResetPasswordTimeOutTest()
    {
        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        SeedData.PopulateTestData(appContext);

        ResetPasswordDto resetDto = new("john@gmail.com", "newpassword", "11111111-1111-1111-1111-111111111111");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/resetpassword/");
        request.Content = JsonContent.Create(resetDto);
        
        await Task.Delay(5000);
        var response = await _client.SendAsync(request);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Reset time is out.", details.Detail);
    }

    [Fact]
    public async Task ResetPasswordValidationErrorTest()
    {
        ResetPasswordDto resetDto = new("jfhgjail.com", "newpassword", "11111111-2222-3333-4444-555555555555");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/resetpassword/");
        request.Content = JsonContent.Create(resetDto);

        var response = await _client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Email\":[\"'Email' is not a valid email address.\"]}", details.Extensions.Last().Value.ToString());
    }

}
