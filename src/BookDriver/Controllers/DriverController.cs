using BookDriver.Data;
using BookDriver.Models;
using BookDriver.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookDriver.Controllers;

[Authorize(Roles = "Driver")]
public class DriverController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DriverController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var profile = await GetOwnProfileAsync();
        if (profile is null)
        {
            return NotFound();
        }

        var pendingCount = await _db.Bookings.CountAsync(b => b.DriverProfileId == profile.Id && b.Status == BookingStatus.Pending);

        var vm = new DriverDashboardViewModel
        {
            FullName = profile.User!.FullName,
            VehicleModel = profile.VehicleModel,
            VehiclePlateNumber = profile.VehiclePlateNumber,
            RatePerKm = profile.RatePerKm,
            IsAvailable = profile.IsAvailable,
            Latitude = profile.Latitude,
            Longitude = profile.Longitude,
            LocationUpdatedAt = profile.LocationUpdatedAt,
            PendingRequestCount = pendingCount,
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(DriverDashboardViewModel vm)
    {
        var profile = await GetOwnProfileAsync();
        if (profile is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            vm.FullName = profile.User!.FullName;
            vm.IsAvailable = profile.IsAvailable;
            vm.Latitude = profile.Latitude;
            vm.Longitude = profile.Longitude;
            vm.LocationUpdatedAt = profile.LocationUpdatedAt;
            return View(nameof(Dashboard), vm);
        }

        profile.VehicleModel = vm.VehicleModel.Trim();
        profile.VehiclePlateNumber = vm.VehiclePlateNumber.Trim();
        profile.RatePerKm = vm.RatePerKm;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLocation(double lat, double lng)
    {
        var profile = await GetOwnProfileAsync();
        if (profile is null)
        {
            return NotFound();
        }

        profile.Latitude = lat;
        profile.Longitude = lng;
        profile.LocationUpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability()
    {
        var profile = await GetOwnProfileAsync();
        if (profile is null)
        {
            return NotFound();
        }

        profile.IsAvailable = !profile.IsAvailable;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Dashboard));
    }

    private async Task<DriverProfile?> GetOwnProfileAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        return await _db.DriverProfiles.Include(d => d.User).FirstOrDefaultAsync(d => d.UserId == userId);
    }
}
