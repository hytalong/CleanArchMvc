
using CleanArchMvc.Application.Interface;
using CleanArchMvc.Application.Mappings;
using CleanArchMvc.Application.Services;
using CleanArchMvc.Domain.Interfaces;
using CleanArchMvc.Infra.Data.Context;
using CleanArchMvc.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using CleanArchMvc.Infra.Data.Identity;
using Microsoft.AspNetCore.Identity;
using CleanArchMvc.Domain.Account;
using CleanArchMvc.Messaging.Infrastructure;

namespace CleanArchMvc.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure (this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"
        ), b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
                 options.AccessDeniedPath = "/Account/Login");

        services.AddScoped<ICategoryRepositry, CategoryRespository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        services.AddScoped<IAuthenticate, AuthenticateService>();
        services.AddScoped<ISeedUserRoleInitial,  SeedUserRoleInitial>();

        services.AddAutoMapper(typeof(DomainToDTOMappingProfile));
        services.AddAutoMapper(typeof(DomainToDTOMappingProfile));

        // Se preferir usar EF-based idempotency store em vez do in-memory, registrar aqui:
        // services.AddScoped<IIdempotencyStore, EfIdempotencyStore>();
        var myhandlers = AppDomain.CurrentDomain.Load("CleanArchMvc.Application");

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(myhandlers);
        });

        // Registrar infraestrutura de mensageria em memória (dev/tests). Não liga brokers.
        services.AddInMemoryMessaging();

        return services;
    }
}
