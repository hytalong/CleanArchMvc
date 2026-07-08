using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CleanArchMvc.IoC;

public static class DependencyInjectionJWT
{
    public static IServiceCollection AddInfrastructureJWT(this IServiceCollection services,
        IConfiguration configuration)
    {
        //Informar o tipo de autenticação JWT-Bearer
        //Definir o modelo de desafio de autenticação

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        //Habilita a autenticação JWT usando o esquema e desafio definidos
        //Validar o token
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                //Valores válidos
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"])),
                ClockSkew = TimeSpan.Zero //Evita problemas de expiração do token
            };
        });
        return services;
    }
}
