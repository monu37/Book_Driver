using System.ComponentModel.DataAnnotations;

namespace BookDriver.ViewModels;

public class ProfileViewModel
{
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, Phone]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}
