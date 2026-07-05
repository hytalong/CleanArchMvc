using System.ComponentModel.DataAnnotations;

namespace CleanArchMvc.API.Models;

public class LoginModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(20, ErrorMessage = "Password must be at least {2} and at max " +
        "{1} characters long.", MinimumLength = 10)]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
