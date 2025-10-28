using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using Infraestructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CuidarMed_.Tests;

public class JwtValidationMiddlewareTests
{
    private static IConfiguration BuildConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "CuidarMed",
            ["Jwt:Audience"] = "CuidarMedClients",
            ["Jwt:SecretKey"] = "b7b53f0e9cde4a5daff8df4a8e4c993f4e2d1b16f22a447fb041b6f61ea44f73"
        })
        .Build();

    [Fact]
    public async Task InvokeAsync_WithValidToken_AllowsRequest()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/v1/doctors" }
        };
        httpContext.Response.Body = new MemoryStream();

        var token = CreateToken(expires: DateTime.UtcNow.AddMinutes(5));
        httpContext.Request.Headers.Authorization = $"Bearer {token}";

        var invoked = false;
        Task Next(HttpContext context)
        {
            invoked = true;
            Assert.True(context.User.Identity?.IsAuthenticated);
            Assert.Equal("123", context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Task.CompletedTask;
        }

        var middleware = new JwtValidationMiddleware(Next, BuildConfiguration(), NullLogger<JwtValidationMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.True(invoked);
        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode == 0 ? StatusCodes.Status200OK : httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithExpiredToken_Returns401()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/v1/doctors" }
        };
        httpContext.Response.Body = new MemoryStream();

        var token = CreateToken(expires: DateTime.UtcNow.AddMinutes(-5));
        httpContext.Request.Headers.Authorization = $"Bearer {token}";

        var middleware = new JwtValidationMiddleware(_ => Task.CompletedTask, BuildConfiguration(), NullLogger<JwtValidationMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidToken_Returns401()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/v1/doctors" }
        };
        httpContext.Response.Body = new MemoryStream();
        httpContext.Request.Headers.Authorization = "Bearer invalid.token.value";

        var middleware = new JwtValidationMiddleware(_ => Task.CompletedTask, BuildConfiguration(), NullLogger<JwtValidationMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, httpContext.Response.StatusCode);
    }

    private static string CreateToken(DateTime expires)
    {
        var secretKey = "b7b53f0e9cde4a5daff8df4a8e4c993f4e2d1b16f22a447fb041b6f61ea44f73";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "CuidarMed",
            audience: "CuidarMedClients",
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123"),
                new Claim(ClaimTypes.Email, "doctor@example.com")
            },
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
