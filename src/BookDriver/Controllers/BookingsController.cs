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
            YearsOfExperience = driver.YearsOfExperience,
            RatePerHour = driver.RatePerHour,
            DistanceKm = distanceKm,
            EstimatedFare = Math.Round(driver.RatePerHour * 2, 2),
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
            vm.YearsOfExperience = driver.YearsOfExperience;
            vm.RatePerHour = driver.RatePerHour;
            return View(vm);
        }

        var distanceKm = GeoService.DistanceKm(vm.PickupLatitude, vm.PickupLongitude, driver.Latitude, driver.Longitude);
        var userId = _userManager.GetUserId(User)!;

        var booking = new Booking
        {
            CustomerId = userId,
            DriverProfileId = driver.Id,
            CarModel = vm.CarModel,
            CarNumber = vm.CarNumber,
            PickupAddress = vm.PickupAddress,
            PickupLatitude = vm.PickupLatitude,
            PickupLongitude = vm.PickupLongitude,
            DropoffAddress = vm.DropoffAddress,
            TripStartAt = vm.TripStartAt,
            EstimatedHours = vm.EstimatedHours,
            DistanceKm = distanceKm,
            EstimatedFare = Math.Round(driver.RatePerHour * (decimal)vm.EstimatedHours, 2),
            Status = BookingStatus.Pending,
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Booking request sent! The driver will confirm shortly.";
        return RedirectToAction(nameof(Mine));
    }

    private static readonly BookingStatus[] TerminalStatuses =
    {
        BookingStatus.Completed, BookingStatus.Rejected, BookingStatus.Cancelled,
    };

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Mine()
    {
        var userId = _userManager.GetUserId(User)!;

        var items = await _db.Bookings
            .Include(b => b.DriverProfile).ThenInclude(d => d!.User)
            .Where(b => b.CustomerId == userId && !TerminalStatuses.Contains(b.Status))
            .OrderByDescending(b => b.RequestedAt)
            .Select(b => new BookingListItemViewModel
            {
                Id = b.Id,
                OtherPartyName = b.DriverProfile!.User!.FullName,
                CarModel = b.CarModel,
                CarNumber = b.CarNumber,
                PickupAddress = b.PickupAddress,
                DropoffAddress = b.DropoffAddress,
                TripStartAt = b.TripStartAt,
                EstimatedHours = b.EstimatedHours,
                Status = b.Status,
                DistanceKm = b.DistanceKm,
                EstimatedFare = b.EstimatedFare,
                RequestedAt = b.RequestedAt,
                CustomerRating = b.CustomerRating,
                CustomerRatingComment = b.CustomerRatingComment,
                DriverRating = b.DriverRating,
                DriverRatingComment = b.DriverRatingComment,
            })
            .ToListAsync();

        return View(items);
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> History()
    {
        var userId = _userManager.GetUserId(User)!;

        var items = await _db.Bookings
            .Include(b => b.DriverProfile).ThenInclude(d => d!.User)
            .Where(b => b.CustomerId == userId && TerminalStatuses.Contains(b.Status))
            .OrderByDescending(b => b.RequestedAt)
            .Select(b => new BookingListItemViewModel
            {
                Id = b.Id,
                OtherPartyName = b.DriverProfile!.User!.FullName,
                CarModel = b.CarModel,
                CarNumber = b.CarNumber,
                PickupAddress = b.PickupAddress,
                DropoffAddress = b.DropoffAddress,
                TripStartAt = b.TripStartAt,
                EstimatedHours = b.EstimatedHours,
                Status = b.Status,
                DistanceKm = b.DistanceKm,
                EstimatedFare = b.EstimatedFare,
                RequestedAt = b.RequestedAt,
                CustomerRating = b.CustomerRating,
                CustomerRatingComment = b.CustomerRatingComment,
                DriverRating = b.DriverRating,
                DriverRatingComment = b.DriverRatingComment,
            })
            .ToListAsync();

        return View(items);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RateDriver(RateBookingViewModel vm)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _db.Bookings.Include(b => b.DriverProfile)
            .FirstOrDefaultAsync(b => b.Id == vm.BookingId && b.CustomerId == userId);

        if (booking is not null && booking.Status == BookingStatus.Completed && booking.CustomerRating is null)
        {
            booking.CustomerRating = vm.Rating;
            booking.CustomerRatingComment = vm.Comment;

            var driver = booking.DriverProfile!;
            var ratings = await _db.Bookings
                .Where(b => b.DriverProfileId == driver.Id && b.CustomerRating != null)
                .Select(b => b.CustomerRating!.Value)
                .ToListAsync();
            ratings.Add(vm.Rating);
            driver.AverageRating = ratings.Average();
            driver.RatingCount = ratings.Count;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Thanks for rating your driver!";
        }

        return RedirectToAction(nameof(History));
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
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Track(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _db.Bookings.Include(b => b.DriverProfile).ThenInclude(d => d!.User)
            .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == userId);

        if (booking is null)
        {
            return NotFound();
        }

        if (booking.Status != BookingStatus.Accepted && booking.Status != BookingStatus.Completed)
        {
            TempData["Error"] = "Tracking is only available once a driver has accepted your ride.";
            return RedirectToAction(nameof(Mine));
        }

        var vm = new TrackBookingViewModel
        {
            BookingId = booking.Id,
            DriverName = booking.DriverProfile!.User!.FullName,
            PickupAddress = booking.PickupAddress,
            DropoffAddress = booking.DropoffAddress,
            Status = booking.Status,
            DriverLatitude = booking.DriverProfile.Latitude,
            DriverLongitude = booking.DriverProfile.Longitude,
            PickupLatitude = booking.PickupLatitude,
            PickupLongitude = booking.PickupLongitude,
        };

        return View(vm);
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> DriverLocation(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _db.Bookings.Include(b => b.DriverProfile)
            .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == userId);

        if (booking is null || (booking.Status != BookingStatus.Accepted && booking.Status != BookingStatus.Completed))
        {
            return NotFound();
        }

        return Json(new
        {
            lat = booking.DriverProfile!.Latitude,
            lng = booking.DriverProfile.Longitude,
            updatedAt = booking.DriverProfile.LocationUpdatedAt,
            status = booking.Status.ToString(),
        });
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
            .Where(b => b.DriverProfileId == driverProfile.Id && !TerminalStatuses.Contains(b.Status))
            .OrderByDescending(b => b.RequestedAt)
            .Select(b => new BookingListItemViewModel
            {
                Id = b.Id,
                OtherPartyName = b.Customer!.FullName,
                CarModel = b.CarModel,
                CarNumber = b.CarNumber,
                PickupAddress = b.PickupAddress,
                DropoffAddress = b.DropoffAddress,
                TripStartAt = b.TripStartAt,
                EstimatedHours = b.EstimatedHours,
                Status = b.Status,
                DistanceKm = b.DistanceKm,
                EstimatedFare = b.EstimatedFare,
                RequestedAt = b.RequestedAt,
                CustomerRating = b.CustomerRating,
                CustomerRatingComment = b.CustomerRatingComment,
                DriverRating = b.DriverRating,
                DriverRatingComment = b.DriverRatingComment,
            })
            .ToListAsync();

        return View(items);
    }

    [HttpGet]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> DriverHistory()
    {
        var userId = _userManager.GetUserId(User)!;
        var driverProfile = await _db.DriverProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
        if (driverProfile is null)
        {
            return RedirectToAction("Dashboard", "Driver");
        }

        var items = await _db.Bookings
            .Include(b => b.Customer)
            .Where(b => b.DriverProfileId == driverProfile.Id && TerminalStatuses.Contains(b.Status))
            .OrderByDescending(b => b.RequestedAt)
            .Select(b => new BookingListItemViewModel
            {
                Id = b.Id,
                OtherPartyName = b.Customer!.FullName,
                CarModel = b.CarModel,
                CarNumber = b.CarNumber,
                PickupAddress = b.PickupAddress,
                DropoffAddress = b.DropoffAddress,
                TripStartAt = b.TripStartAt,
                EstimatedHours = b.EstimatedHours,
                Status = b.Status,
                DistanceKm = b.DistanceKm,
                EstimatedFare = b.EstimatedFare,
                RequestedAt = b.RequestedAt,
                CustomerRating = b.CustomerRating,
                CustomerRatingComment = b.CustomerRatingComment,
                DriverRating = b.DriverRating,
                DriverRatingComment = b.DriverRatingComment,
            })
            .ToListAsync();

        return View(items);
    }

    [HttpPost]
    [Authorize(Roles = "Driver")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RateCustomer(RateBookingViewModel vm)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _db.Bookings.Include(b => b.DriverProfile)
            .FirstOrDefaultAsync(b => b.Id == vm.BookingId && b.DriverProfile!.UserId == userId);

        if (booking is not null && booking.Status == BookingStatus.Completed && booking.DriverRating is null)
        {
            booking.DriverRating = vm.Rating;
            booking.DriverRatingComment = vm.Comment;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Thanks for rating your passenger!";
        }

        return RedirectToAction(nameof(DriverHistory));
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

        return RedirectToAction(nameof(DriverHistory));
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
