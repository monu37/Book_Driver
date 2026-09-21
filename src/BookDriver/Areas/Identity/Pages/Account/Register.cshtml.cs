using System.ComponentModel.DataAnnotations;
using BookDriver.Data;
using BookDriver.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookDriver.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        ILogger<RegisterModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "I want to")]
        public string AccountType { get; set; } = "Customer";

        [StringLength(30)]
        [Display(Name = "Driving license number")]
        public string? LicenseNumber { get; set; }

        [Range(0, 60)]
        [Display(Name = "Years of experience")]
        public int YearsOfExperience { get; set; }

        [Range(1, 2000)]
        [Display(Name = "Rate per hour (₹)")]
        public decimal RatePerHour { get; set; } = 150m;

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (Input.AccountType == "Driver")
        {
            if (string.IsNullOrWhiteSpace(Input.LicenseNumber))
            {
                ModelState.AddModelError("Input.LicenseNumber", "Driving license number is required for drivers.");
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            FullName = Input.FullName,
            PhoneNumber = Input.PhoneNumber,
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User created a new account with password.");

            var role = Input.AccountType == "Driver" ? "Driver" : "Customer";
            await _userManager.AddToRoleAsync(user, role);

            if (role == "Driver")
            {
                _db.DriverProfiles.Add(new DriverProfile
                {
                    UserId = user.Id,
                    LicenseNumber = Input.LicenseNumber!.Trim(),
                    YearsOfExperience = Input.YearsOfExperience,
                    RatePerHour = Input.RatePerHour,
                    IsAvailable = false,
                    Latitude = 0,
                    Longitude = 0,
                });
                await _db.SaveChangesAsync();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            if (role == "Driver")
            {
                return LocalRedirect("~/Driver/Dashboard");
            }

            return LocalRedirect(returnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}
