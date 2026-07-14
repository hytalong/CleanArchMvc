using CleanArchMvc.Domain.Account;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace CleanArchMvc.Infra.Data.Identity;

public class SeedUserRoleInitial : ISeedUserRoleInitial
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public SeedUserRoleInitial(RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedUsersAsync()
    {
        if (await _userManager.FindByEmailAsync("usuario@localhost") == null)
        {
            var user = new ApplicationUser
            {
                UserName = "usuario@localhost",
                Email = "usuario@localhost",
                NormalizedUserName = "USUARIO@LOCALHOST",
                NormalizedEmail = "USUARIO@LOCALHOST",
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, "Numsey#2021");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
        }

        if (await _userManager.FindByEmailAsync("admin@localhost") == null)
        {
            var user = new ApplicationUser
            {
                UserName = "admin@localhost",
                Email = "admin@localhost",
                NormalizedUserName = "ADMIN@LOCALHOST",
                NormalizedEmail = "ADMIN@LOCALHOST",
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, "Numsey#2021");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }

    public async Task SeedRolesAsync()
    {
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            var role = new IdentityRole { Name = "User", NormalizedName = "USER" };
            await _roleManager.CreateAsync(role);
        }

        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            var role = new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" };
            await _roleManager.CreateAsync(role);
        }
    }

}
