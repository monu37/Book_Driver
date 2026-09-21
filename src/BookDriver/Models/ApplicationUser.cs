using Microsoft.AspNetCore.Identity;

namespace BookDriver.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
