using CleanArchMvc.Domain.Account;
using CleanArchMvc.IoC;

namespace CleanArchMvc.WebUI;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // Registrar serviços
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddInfrastructure(Configuration);
        services.AddControllersWithViews();
    }

    // Configurar pipeline HTTP
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
        ISeedUserRoleInitial seedUserRoleInitial)
    {
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        seedUserRoleInitial.SeedRoles();
        seedUserRoleInitial.SeedUsers();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
