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
            new { Name = "Ravi Kumar", Email = "ravi.driver@bookdriver.demo", Vehicle = "Maruti Suzuki Swift", Plate = "DL3CAB1234", Rate = 14m, Lat = 28.6139, Lng = 77.2090 },
            new { Name = "Suresh Yadav", Email = "suresh.driver@bookdriver.demo", Vehicle = "Hyundai i20", Plate = "DL5CAF5678", Rate = 15m, Lat = 28.6304, Lng = 77.2177 },
            new { Name = "Amit Sharma", Email = "amit.driver@bookdriver.demo", Vehicle = "Toyota Innova", Plate = "DL1CAK9012", Rate = 22m, Lat = 28.5921, Lng = 77.2290 },
            new { Name = "Deepak Singh", Email = "deepak.driver@bookdriver.demo", Vehicle = "Honda City", Plate = "DL8CAM3456", Rate = 18m, Lat = 28.6448, Lng = 77.1925 },
            new { Name = "Manoj Verma", Email = "manoj.driver@bookdriver.demo", Vehicle = "Tata Tiago", Plate = "DL2CAX7890", Rate = 13m, Lat = 28.5706, Lng = 77.3272 },
        };

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

            db.DriverProfiles.Add(new DriverProfile
            {
                UserId = user.Id,
                VehicleModel = d.Vehicle,
                VehiclePlateNumber = d.Plate,
                RatePerKm = d.Rate,
                IsAvailable = true,
                Latitude = d.Lat,
                Longitude = d.Lng,
                LocationUpdatedAt = DateTime.UtcNow,
            });
        }

        await db.SaveChangesAsync();
    }
}
