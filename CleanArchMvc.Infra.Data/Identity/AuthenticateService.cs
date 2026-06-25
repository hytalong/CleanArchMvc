using CleanArchMvc.Domain.Account;
using Microsoft.AspNetCore.Identity;

namespace CleanArchMvc.Infra.Data.Identity;

public class AuthenticateService : IAuthenticate
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _singInManager;
    public AuthenticateService(SignInManager<ApplicationUser> singInManager,
        UserManager<ApplicationUser> userManager)
    {
        _singInManager = singInManager;
        _userManager = userManager;
    }

    public async Task<bool> Authenticate(string email, string password)
    {
        var result = await _singInManager.PasswordSignInAsync(email, password,
            false, lockoutOnFailure: false);

        return result.Succeeded;
    }
    public async Task<bool> RegisterUser(string email, string password)
    {
        var applicationUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
        };

        var result = await _userManager.CreateAsync(applicationUser, password);

        if (result.Succeeded)
        {
            await _singInManager.SignInAsync(applicationUser, isPersistent: false);
        }

        return result.Succeeded;
    }
    public async Task Logout()
    {
        await _singInManager.SignOutAsync();
    }
}
