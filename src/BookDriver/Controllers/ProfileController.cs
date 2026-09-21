using BookDriver.Models;
using BookDriver.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookDriver.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var vm = new ProfileViewModel
        {
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty,
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel vm)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            vm.Email = user.Email ?? string.Empty;
            var roles = await _userManager.GetRolesAsync(user);
            vm.Role = roles.FirstOrDefault() ?? string.Empty;
            return View(vm);
        }

        user.FullName = vm.FullName.Trim();
        user.PhoneNumber = vm.PhoneNumber.Trim();
        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }
}
