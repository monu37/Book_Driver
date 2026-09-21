using BookDriver.Data;
using BookDriver.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookDriver.Services;

public static class DbSeeder
{
    public static readonly string[] Roles = { "Customer", "Driver" };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (await db.DriverProfiles.AnyAsync())
        {
            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var demoDrivers = new[]
        {
            new { Name = "Ravi Kumar", Email = "ravi.driver@bookdriver.demo", License = "DL-0420110012345", Experience = 8, Rate = 150m, Lat = 28.6139, Lng = 77.2090 },
            new { Name = "Suresh Yadav", Email = "suresh.driver@bookdriver.demo", License = "DL-0420110023456", Experience = 5, Rate = 130m, Lat = 28.6304, Lng = 77.2177 },
            new { Name = "Amit Sharma", Email = "amit.driver@bookdriver.demo", License = "DL-0420110034567", Experience = 12, Rate = 180m, Lat = 28.5921, Lng = 77.2290 },
            new { Name = "Deepak Singh", Email = "deepak.driver@bookdriver.demo", License = "DL-0420110045678", Experience = 3, Rate = 120m, Lat = 28.6448, Lng = 77.1925 },
            new { Name = "Manoj Verma", Email = "manoj.driver@bookdriver.demo", License = "DL-0420110056789", Experience = 6, Rate = 140m, Lat = 28.5706, Lng = 77.3272 },
        };

        var driverProfiles = new List<DriverProfile>();

        foreach (var d in demoDrivers)
        {
            var user = new ApplicationUser
            {
                UserName = d.Email,
                Email = d.Email,
                FullName = d.Name,
                EmailConfirmed = true,
                PhoneNumber = "9999999999",
            };

            var result = await userManager.CreateAsync(user, "Driver@123");
            if (!result.Succeeded)
            {
                continue;
            }

            await userManager.AddToRoleAsync(user, "Driver");

            var profile = new DriverProfile
            {
                UserId = user.Id,
                LicenseNumber = d.License,
                YearsOfExperience = d.Experience,
                RatePerHour = d.Rate,
                IsAvailable = true,
                Latitude = d.Lat,
                Longitude = d.Lng,
                LocationUpdatedAt = DateTime.UtcNow,
            };
            db.DriverProfiles.Add(profile);
            driverProfiles.Add(profile);
        }

        await db.SaveChangesAsync();
        await SeedDemoRideHistoryAsync(db, userManager, driverProfiles);
    }

    private static async Task SeedDemoRideHistoryAsync(
        ApplicationDbContext db, UserManager<ApplicationUser> userManager, List<DriverProfile> drivers)
    {
        var demoCustomer = new ApplicationUser
        {
            UserName = "demo.customer@bookdriver.demo",
            Email = "demo.customer@bookdriver.demo",
            FullName = "Anita Rao",
            EmailConfirmed = true,
            PhoneNumber = "9999900000",
        };

        var result = await userManager.CreateAsync(demoCustomer, "Customer@123");
        if (!result.Succeeded || drivers.Count < 2)
        {
            return;
        }

        await userManager.AddToRoleAsync(demoCustomer, "Customer");

        var pastRides = new[]
        {
            new { Driver = drivers[0], Rating = 5, Comment = "Great driver, very safe and punctual!", DaysAgo = 6 },
            new { Driver = drivers[0], Rating = 4, Comment = "Good ride, arrived a little late.", DaysAgo = 3 },
            new { Driver = drivers[1], Rating = 5, Comment = "Excellent, would book again.", DaysAgo = 1 },
        };

        foreach (var r in pastRides)
        {
            var requestedAt = DateTime.UtcNow.AddDays(-r.DaysAgo);
            db.Bookings.Add(new Booking
            {
                CustomerId = demoCustomer.Id,
                DriverProfileId = r.Driver.Id,
                CarModel = "Maruti Suzuki Baleno",
                CarNumber = "DL7CAQ2468",
                PickupAddress = "12 Nehru Place",
                PickupLatitude = r.Driver.Latitude,
                PickupLongitude = r.Driver.Longitude,
                DropoffAddress = "Cyber Hub, Gurugram",
                TripStartAt = requestedAt.AddHours(1),
                EstimatedHours = 3,
                DistanceKm = 2.5,
                EstimatedFare = r.Driver.RatePerHour * 3,
                Status = BookingStatus.Completed,
                RequestedAt = requestedAt,
                RespondedAt = requestedAt.AddMinutes(5),
                CompletedAt = requestedAt.AddHours(3),
                CustomerRating = r.Rating,
                CustomerRatingComment = r.Comment,
            });
        }

        await db.SaveChangesAsync();

        foreach (var driver in drivers.Take(2))
        {
            var ratings = await db.Bookings
                .Where(b => b.DriverProfileId == driver.Id && b.CustomerRating != null)
                .Select(b => b.CustomerRating!.Value)
                .ToListAsync();

            if (ratings.Count > 0)
            {
                driver.AverageRating = ratings.Average();
                driver.RatingCount = ratings.Count;
            }
        }

        await db.SaveChangesAsync();
    }
}
