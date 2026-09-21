using System.ComponentModel.DataAnnotations;

namespace BookDriver.Models;

public class DriverProfile
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Required, StringLength(100)]
    public string VehicleModel { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string VehiclePlateNumber { get; set; } = string.Empty;

    [Range(1, 500)]
    public decimal RatePerKm { get; set; } = 15m;

    public bool IsAvailable { get; set; } = true;

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime LocationUpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
