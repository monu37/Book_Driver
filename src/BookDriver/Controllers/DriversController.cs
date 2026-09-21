using BookDriver.Data;
using BookDriver.Services;
using BookDriver.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookDriver.Controllers;

public class DriversController : Controller
{
    private readonly ApplicationDbContext _db;

    public DriversController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Nearby(double? lat, double? lng, double radiusKm = 10)
    {
        var vm = new NearbySearchViewModel { RadiusKm = radiusKm };

        if (lat is null || lng is null)
        {
            return View(vm);
        }

        vm.Latitude = lat.Value;
        vm.Longitude = lng.Value;

        var availableDrivers = await _db.DriverProfiles
            .Include(d => d.User)
            .Where(d => d.IsAvailable)
            .ToListAsync();

        vm.Drivers = availableDrivers
            .Select(d => new NearbyDriverViewModel
            {
                DriverProfileId = d.Id,
                FullName = d.User!.FullName,
                VehicleModel = d.VehicleModel,
                VehiclePlateNumber = d.VehiclePlateNumber,
                RatePerKm = d.RatePerKm,
                DistanceKm = GeoService.DistanceKm(lat.Value, lng.Value, d.Latitude, d.Longitude),
            })
            .Where(d => d.DistanceKm <= radiusKm)
            .OrderBy(d => d.DistanceKm)
            .ToList();

        return View(vm);
    }
}
