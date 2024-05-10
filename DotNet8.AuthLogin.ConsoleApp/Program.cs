using DotNet8.AuthLogin.ConsoleApp;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var user = new { Email = "mackk5504@gmail.com", Password = "123" };

app.UseAuthenticationMiddleware();

app.MapPost("/login", async (HttpContext context) =>
{
    var model = await context.Request.ReadFromJsonAsync<LoginViewModel>();

    if (model?.Email == user.Email && model?.Password == user.Password)
    {
        var claims = new[] { new Claim(ClaimTypes.Name, user.Email) };

        var serializedClaims = JsonSerializer.Serialize(claims);

        context.Response.Cookies.Append("auth_token", serializedClaims, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddMinutes(10),
            HttpOnly = true
        });

        return Results.Ok("User authenticated successfully.");
    }
    else
    {
        return Results.BadRequest("Invalid email or password.");
    }
});

app.MapGet("/logout", (HttpContext context) =>
{
    context.Response.Cookies.Delete("auth_token");

    return Results.Ok("User logged out successfully.");
});

app.MapGet("/", () =>
{
    return Results.Ok("Welcome to the authentication API.");
});

app.Run();