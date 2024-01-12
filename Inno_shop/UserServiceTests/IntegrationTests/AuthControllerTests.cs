using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Net;
using UserService.Application.Dtos;
using UserServiceTests.IntegrationTests.Helpers;
using UserService;
using System.Text.Json;

namespace UserServiceTests.IntegrationTests;

[Collection("Sequential")]
public class AuthControllerTests : IClassFixture<CustomFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public void RegisterValidTest()
    {
        var user = new UserRegisterDto("pfjggfjfjjr", "johhhhhhn@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/register/");
        request.Content = JsonContent.Create(user);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users/");
        getRequest.Headers.Add("Authorization", "Bearer " + JwtGenerator.GenerateJwt("john@gmail.com", "136DA01F-9ABD-4d9d-80C7-02AF85C822A2").Result);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;

        var getResponse = _client.SendAsync(getRequest).Result;
        var getResponseString = getResponse.Content.ReadAsStringAsync().Result;
        var getResult = JsonSerializer.Deserialize<List<UserDto>>(getResponseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Registration was successful. Check email.", responseString);
        Assert.Equal(4, getResult.Count);

        SeedData.ResetData();
    }
    [Fact]
    public void RegisterUsedEmailTest()
    {
        var user = new UserRegisterDto("pfjggfjfjjr", "john@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/register/");
        request.Content = JsonContent.Create(user);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Email is already in use.", details.Detail);
    }

    [Fact]
    public void RegisterValidationErrorTest()
    {
        var user = new UserRegisterDto("pfjggfjfjjr", "chghgchc@gmail.com", "");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/register/");
        request.Content = JsonContent.Create(user);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Password\":[\"The length of 'Password' must be at least 8 characters. You entered 0 characters.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public void LoginValidTest()
    {
        var user = new LoginDto("john@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login/");
        request.Content = JsonContent.Create(user);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var responseDto = JsonSerializer.Deserialize<LoginResponseDto>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseDto.JwtToken);

        SeedData.ResetData();
    }

    [Fact]
    public void LoginNotExistingEmailTest()
    {
        var user = new LoginDto("fchgvjhb@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login/");
        request.Content = JsonContent.Create(user);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public void LoginInvalidPasswordTest()
    {
        var user = new LoginDto("john@gmail.com", "passsssssssssssword");
        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login/");
        request.Content = JsonContent.Create(user);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Invalid password.", details.Detail);
    }

    [Fact]
    public void LoginNotConfirmedEmailTest()
    {
        var user = new LoginDto("nameee@gmail.com", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login/");
        request.Content = JsonContent.Create(user);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Email is not confirmed.", details.Detail);
    }

    [Fact]
    public void LoginValidationErrorTest()
    {
        var user = new LoginDto("johfhgvjilcom", "password");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login/");
        request.Content = JsonContent.Create(user);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Email\":[\"'Email' is not a valid email address.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public void ForgotPasswordValidTest()
    {
        var email = "john@gmail.com";

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/auth/forgotpassword/?email={email}");
        request.Content = JsonContent.Create(email);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"Check {email} email. You may now reset your password whithin 1 hour.", responseString);

        SeedData.ResetData();
    }

    [Fact]
    public void ForgotPasswordValidationErrorTest()
    {
        var email = "johfhgailcom";

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/auth/forgotpassword/?email={email}");
        request.Content = JsonContent.Create(email);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"email\":[\"The email field is not a valid e-mail address.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public void ForgotPasswordNotExistingTest()
    {
        var email = "joooooohn@gmail.com";

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/auth/forgotpassword/?email={email}");
        request.Content = JsonContent.Create(email);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public void ConfirmEmailValidTest()
    {
        var email = "nameee@gmail.com";
        var token = "11111111-2222-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/confirmemail?email={email}&token={token}");

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Confirmation was successful.", responseString);

        SeedData.ResetData();
    }

    [Fact]
    public void ConfirmEmailNotExistingTest()
    {
        var email = "joooohn@gmail.com";
        var token = "11111111-2222-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/confirmemail?email={email}&token={token}");

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public void ConfirmConfirmedEmailTest()
    {
        var email = "john@gmail.com";
        var token = "11111111-2222-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/confirmemail?email={email}&token={token}");

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Email is already confirmed.", details.Detail);
    }

    [Fact]
    public void ConfirmEmailInvalidTokenTest()
    {
        var email = "nameee@gmail.com";
        var token = "11111111-0000-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/confirmemail?email={email}&token={token}");

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Invalid token.", details.Detail);
    }

    [Fact]
    public void ConfirmEmailValidationErrorTest()
    {
        var email = "namhgvjhbail.com";
        var token = "11111111-2222-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/confirmemail?email={email}&token={token}");

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"email\":[\"The email field is not a valid e-mail address.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public void GetResetPasswordValidTest()
    {
        var email = "nameee@gmail.com";
        var token = "11111111-2222-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/resetpassword?email={email}&token={token}");

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var resetDto = JsonSerializer.Deserialize<ResetPasswordDto>(responseString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(email, resetDto.Email);
    }

    [Fact]
    public void GetResetPasswordValidationErrorTest()
    {
        var email = "namhgvjhbail.com";
        var token = "11111111-2222-3333-4444-555555555555";

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/resetpassword?email={email}&token={token}");

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"email\":[\"The email field is not a valid e-mail address.\"]}", details.Extensions.Last().Value.ToString());
    }

    [Fact]
    public void ResetPasswordPostValidTest()
    {
        ResetPasswordDto resetDto = new("john@gmail.com", "newpassword", "11111111-1111-1111-1111-111111111111");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/resetpassword/");
        request.Content = JsonContent.Create(resetDto);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Password has been successfully changed.", responseString);

        SeedData.ResetData();
    }

    [Fact]
    public void ResetPasswordPostNotExistingTest()
    {
        ResetPasswordDto resetDto = new("johhhhhhn@gmail.com", "newpassword", "11111111-2222-3333-4444-555555555555");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/resetpassword/");
        request.Content = JsonContent.Create(resetDto);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User not found.", details.Detail);
    }

    [Fact]
    public void ResetPasswordInvalidTokenTest()
    {
        ResetPasswordDto resetDto = new("john@gmail.com", "newpassword", "11111111-0000-3333-4444-555555555555");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/resetpassword/");
        request.Content = JsonContent.Create(resetDto);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Invalid token.", details.Detail);
    }

    [Fact]
    public void ResetPasswordTimeOutTest()
    {
        ResetPasswordDto resetDto = new("john@gmail.com", "newpassword", "11111111-1111-1111-1111-111111111111");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/resetpassword/");
        request.Content = JsonContent.Create(resetDto);

        Thread.Sleep(5000);
        var response = _client.SendAsync(request).Result;

        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Reset time is out.", details.Detail);
    }

    [Fact]
    public void ResetPasswordValidationErrorTest()
    {
        ResetPasswordDto resetDto = new("jfhgjail.com", "newpassword", "11111111-2222-3333-4444-555555555555");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/resetpassword/");
        request.Content = JsonContent.Create(resetDto);

        var response = _client.SendAsync(request).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var details = JsonSerializer.Deserialize<ProblemDetails>(responseString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Email\":[\"'Email' is not a valid email address.\"]}", details.Extensions.Last().Value.ToString());
    }
}
