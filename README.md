# BookDriver

Hire a chauffeur near you — a driver who drives *your own car*, not theirs.
An ASP.NET Core 8 MVC app with ASP.NET Core Identity auth, EF Core (SQLite),
and browser-geolocation-based "drivers near me" search.

## Features

- Two account types on signup: **Customer** (books a driver) and **Driver**
  (registers a license and accepts ride requests — no vehicle needed, since
  they drive the customer's car).
- Customers grab their current location from the browser and see available
  drivers within a chosen radius, sorted by distance (Haversine formula),
  along with each driver's experience and hourly rate.
- Booking a driver asks for the customer's own car (model + registration
  number), pickup/drop-off addresses, when the trip starts, and how many
  hours the driver is needed for; fare is estimated as rate/hour × hours.
- Drivers see incoming requests and can accept, reject, or mark a ride
  completed.
- Drivers have a dashboard to go online/offline, push their live location,
  and edit their license, experience, and hourly rate.
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

- `Models/` — `ApplicationUser`, `DriverProfile` (license, experience, rate/hour,
  live location), `Booking` (customer's car, trip window, fare), `BookingStatus`.
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
- Fares shown are a simple estimate (`rate per hour × hours booked`) for
  demo purposes, not a production pricing engine.
