using BookDriver.Data;
using BookDriver.Models;
using BookDriver.Services;
using BookDriver.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookDriver.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create(int driverProfileId, double lat, double lng)
    {
        var driver = await _db.DriverProfiles.Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == driverProfileId && d.IsAvailable);

        if (driver is null)
        {
            TempData["Error"] = "That driver is no longer available.";
            return RedirectToAction("Nearby", "Drivers");
        }

        var distanceKm = GeoService.DistanceKm(lat, lng, driver.Latitude, driver.Longitude);

        var vm = new CreateBookingViewModel
        {
            DriverProfileId = driver.Id,
            DriverName = driver.User!.FullName,
            VehicleModel = driver.VehicleModel,
            RatePerKm = driver.RatePerKm,
            DistanceKm = distanceKm,
            EstimatedFare = Math.Round(driver.RatePerKm * (decimal)Math.Max(distanceKm, 2), 2),
            PickupLatitude = lat,
            PickupLongitude = lng,
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookingViewModel vm)
    {
        var driver = await _db.DriverProfiles.Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == vm.DriverProfileId);

        if (driver is null || !driver.IsAvailable)
        {
            TempData["Error"] = "That driver is no longer available.";
            return RedirectToAction("Nearby", "Drivers");
        }

        if (!ModelState.IsValid)
        {
            vm.DriverName = driver.User!.FullName;
            vm.VehicleModel = driver.VehicleModel;
            vm.RatePerKm = driver.RatePerKm;
            return View(vm);
        }

        var distanceKm = GeoService.DistanceKm(vm.PickupLatitude, vm.PickupLongitude, driver.Latitude, driver.Longitude);
        var userId = _userManager.GetUserId(User)!;

        var booking = new Booking
        {
            CustomerId = userId,
            DriverProfileId = driver.Id,
            PickupAddress = vm.PickupAddress,
            PickupLatitude = vm.PickupLatitude,
            PickupLongitude = vm.PickupLongitude,
            DropoffAddress = vm.DropoffAddress,
            DistanceKm = distanceKm,
            EstimatedFare = Math.Round(driver.RatePerKm * (decimal)Math.Max(distanceKm, 2), 2),
            Status = BookingStatus.Pending,
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Booking request sent! The driver will confirm shortly.";
        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Mine()
    {
        var userId = _userManager.GetUserId(User)!;

        var items = await _db.Bookings
            .Include(b => b.DriverProfile).ThenInclude(d => d!.User)
            .Where(b => b.CustomerId == userId)
            .OrderByDescending(b => b.RequestedAt)
            .Select(b => new BookingListItemViewModel
            {
                Id = b.Id,
                OtherPartyName = b.DriverProfile!.User!.FullName,
                VehicleModel = b.DriverProfile.VehicleModel,
                PickupAddress = b.PickupAddress,
                DropoffAddress = b.DropoffAddress,
                Status = b.Status,
                DistanceKm = b.DistanceKm,
                EstimatedFare = b.EstimatedFare,
                RequestedAt = b.RequestedAt,
            })
            .ToListAsync();

        return View(items);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == userId);

        if (booking is not null && booking.Status == BookingStatus.Pending)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.RespondedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> Requests()
    {
        var userId = _userManager.GetUserId(User)!;
        var driverProfile = await _db.DriverProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
        if (driverProfile is null)
        {
            return RedirectToAction("Dashboard", "Driver");
        }

        var items = await _db.Bookings
            .Include(b => b.Customer)
            .Where(b => b.DriverProfileId == driverProfile.Id)
            .OrderByDescending(b => b.RequestedAt)
            .Select(b => new BookingListItemViewModel
            {
                Id = b.Id,
                OtherPartyName = b.Customer!.FullName,
                VehicleModel = string.Empty,
                PickupAddress = b.PickupAddress,
                DropoffAddress = b.DropoffAddress,
                Status = b.Status,
                DistanceKm = b.DistanceKm,
                EstimatedFare = b.EstimatedFare,
                RequestedAt = b.RequestedAt,
            })
            .ToListAsync();

        return View(items);
    }

    [HttpPost]
    [Authorize(Roles = "Driver")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id) => await RespondAsync(id, BookingStatus.Accepted);

    [HttpPost]
    [Authorize(Roles = "Driver")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id) => await RespondAsync(id, BookingStatus.Rejected);

    [HttpPost]
    [Authorize(Roles = "Driver")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _db.Bookings.Include(b => b.DriverProfile)
            .FirstOrDefaultAsync(b => b.Id == id && b.DriverProfile!.UserId == userId);

        if (booking is not null && booking.Status == BookingStatus.Accepted)
        {
            booking.Status = BookingStatus.Completed;
            booking.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Requests));
    }

    private async Task<IActionResult> RespondAsync(int id, BookingStatus newStatus)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _db.Bookings.Include(b => b.DriverProfile)
            .FirstOrDefaultAsync(b => b.Id == id && b.DriverProfile!.UserId == userId);

        if (booking is not null && booking.Status == BookingStatus.Pending)
        {
            booking.Status = newStatus;
            booking.RespondedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Requests));
    }
}
