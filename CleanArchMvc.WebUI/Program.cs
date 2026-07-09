using CleanArchMvc.Domain.Account;
using CleanArchMvc.Infra.Data.Identity;
using CleanArchMvc.IoC;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if(!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//seedUserRoleInitial.SeedRoles();
//seedUserRoleInitial.SeedUsers();
SeedUserRole(app);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

void SeedUserRole(IApplicationBuilder app)
{
    using (var serviceScope = app.ApplicationServices.CreateScope())
    {
        var seed = serviceScope.ServiceProvider;
        var seedUserRoleInitial = seed
            .GetRequiredService<ISeedUserRoleInitial>();
        seedUserRoleInitial.SeedRoles();
        seedUserRoleInitial.SeedUsers();
    }
}