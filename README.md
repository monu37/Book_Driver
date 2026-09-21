# BookDriver

Book a driver near you. An ASP.NET Core 8 MVC app with ASP.NET Core Identity
auth, EF Core (SQLite), and browser-geolocation-based "drivers near me" search.

## Features

- Two account types on signup: **Customer** (books rides) and **Driver**
  (registers a vehicle and accepts ride requests).
- Customers grab their current location from the browser and see available
  drivers within a chosen radius, sorted by distance (Haversine formula).
- Customers book a driver with pickup/drop-off addresses; drivers see
  incoming requests and can accept, reject, or mark a ride completed.
- Drivers have a dashboard to go online/offline, push their live location,
  and edit their vehicle and rate per km.
- Seed data creates 5 demo drivers around Delhi (`ravi.driver@bookdriver.demo`
  … `manoj.driver@bookdriver.demo`, password `Driver@123`) so the app has
  something to show immediately.

## Running locally

```bash
cd src/BookDriver
dotnet restore
dotnet run
```

The app applies EF Core migrations and seeds demo drivers automatically on
startup. Open the URL printed in the console (e.g. `http://localhost:5000`).

## Project layout

- `Models/` — `ApplicationUser`, `DriverProfile`, `Booking`, `BookingStatus`.
- `Data/ApplicationDbContext.cs` — EF Core Identity + domain DbContext.
- `Controllers/DriversController.cs` — nearby-driver search.
- `Controllers/BookingsController.cs` — create/accept/reject/complete/cancel.
- `Controllers/DriverController.cs` — driver dashboard, location & availability.
- `Areas/Identity/Pages/Account/Register.cshtml(.cs)` — custom registration
  with account-type selection (Customer vs Driver).
- `Services/GeoService.cs` — Haversine distance calculation.
- `Services/DbSeeder.cs` — role + demo driver seeding on startup.

## Notes

- No real email/SMS provider is configured; account confirmation is
  disabled and a no-op `IEmailSender` logs instead of sending mail — swap
  in a real provider before deploying this for real users.
- Fares shown are a simple estimate (`rate per km × distance to pickup`)
  for demo purposes, not a production pricing engine.
