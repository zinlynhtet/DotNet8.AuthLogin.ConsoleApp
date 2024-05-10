using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace DotNet8.AuthLogin.ConsoleApp;

public static class AuthMiddleware
{
    public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var authCookie = context.Request.Cookies["auth_token"];
            if (!string.IsNullOrEmpty(authCookie))
            {
                var claims = JsonSerializer.Deserialize<Claim[]>(authCookie, new JsonSerializerOptions
                {
                    Converters = { new ClaimConverter() }
                });

                var identity = new ClaimsIdentity(claims, "custom");
                context.User = new ClaimsPrincipal(identity);
            }

            await next.Invoke();
        });

        return app;
    }
}

public class ClaimConverter : JsonConverter<Claim>
{
    public override Claim Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            var root = doc.RootElement;
            string type = root.GetProperty("Type").GetString();
            string value = root.GetProperty("Value").GetString();

            return new Claim(type, value);
        }
    }

    public override void Write(Utf8JsonWriter writer, Claim value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Type", value.Type);
        writer.WriteString("Value", value.Value);
        writer.WriteEndObject();
    }
}
