using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Infraestructure.Middleware
{
    public class JwtValidationMiddleware
    {
        private static readonly PathString[] _excludedPaths =
        [
            new PathString("/swagger"),
            new PathString("/favicon.ico")
        ];

        private readonly RequestDelegate _next;
        private readonly ILogger<JwtValidationMiddleware> _logger;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly JwtSecurityTokenHandler _tokenHandler = new();

        public JwtValidationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<JwtValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;

            var issuer = configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer configuration is missing");
            var audience = configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience configuration is missing");
            var secretKey = configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey configuration is missing");

            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (ShouldSkipValidation(context))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("Authorization", out var headerValue) ||
                string.IsNullOrWhiteSpace(headerValue))
            {
                await RejectUnauthorizedAsync(context, "Token no proporcionado");
                return;
            }

            var token = ExtractBearerToken(headerValue!);
            if (token is null)
            {
                await RejectUnauthorizedAsync(context, "Formato de encabezado Authorization inválido");
                return;
            }

            try
            {
                var principal = _tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new SecurityTokenException("Algoritmo de token no soportado");
                }

                context.User = principal;
                await _next(context);
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "Token expirado");
                await RejectUnauthorizedAsync(context, "Token expirado");
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Token inválido");
                await RejectUnauthorizedAsync(context, "Token inválido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al validar el token");
                await RejectUnauthorizedAsync(context, "Error al validar el token");
            }
        }

        private static string? ExtractBearerToken(string authorizationHeader)
        {
            const string prefix = "Bearer ";
            if (!authorizationHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var token = authorizationHeader[prefix.Length..].Trim();
            return string.IsNullOrEmpty(token) ? null : token;
        }

        private static async Task RejectUnauthorizedAsync(HttpContext context, string message)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = message });
        }

        private static bool ShouldSkipValidation(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            {
                return true;
            }

            return _excludedPaths.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase));
        }
    }
}
