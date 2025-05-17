using ChatServer.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ChatServer.Infrastructure
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //Add DB Context
            string connectionStringTemplate = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not set");
            string connectionString = connectionStringTemplate;
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Debug")
            {
                connectionString = connectionStringTemplate
                    .Replace("$SQL_HOST", Environment.GetEnvironmentVariable("SQL_HOST") ?? throw new InvalidOperationException("SQL_HOST is not set"))
                    .Replace("$SQL_PORT", Environment.GetEnvironmentVariable("SQL_PORT") ?? throw new InvalidOperationException("SQL_PORT is not set"))
                    .Replace("$SQL_DB", Environment.GetEnvironmentVariable("SQL_DB") ?? throw new InvalidOperationException("SQL_DB is not set"))
                    .Replace("$SQL_USER", Environment.GetEnvironmentVariable("SQL_USER") ?? throw new InvalidOperationException("SQL_USER is not set"))
                    .Replace("$SQL_PASSWORD", Environment.GetEnvironmentVariable("SQL_PASSWORD") ?? throw new InvalidOperationException("SQL_PASSWORD is not set"));

            }
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                    {
                        var httpClient = new HttpClient();
                        var jwks = httpClient.GetStringAsync(configuration["Jwt:JWKS"]).Result;
                        var keys = new JsonWebKeySet(jwks).Keys;
                        return keys;
                    }
                };
            });
            return services;
        }
    }
}
