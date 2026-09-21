using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BookDriver.Models;

namespace BookDriver.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DriverProfile> DriverProfiles => Set<DriverProfile>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<DriverProfile>()
            .HasIndex(d => d.UserId)
            .IsUnique();

        builder.Entity<DriverProfile>()
            .HasOne(d => d.User)
            .WithOne()
            .HasForeignKey<DriverProfile>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Booking>()
            .HasOne(b => b.Customer)
            .WithMany()
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(b => b.DriverProfile)
            .WithMany(d => d.Bookings)
            .HasForeignKey(b => b.DriverProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DriverProfile>()
            .Property(d => d.RatePerKm)
            .HasColumnType("decimal(10,2)");

        builder.Entity<Booking>()
            .Property(b => b.EstimatedFare)
            .HasColumnType("decimal(10,2)");
    }
}
