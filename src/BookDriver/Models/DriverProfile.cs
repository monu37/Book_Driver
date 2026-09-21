using System.ComponentModel.DataAnnotations;

namespace BookDriver.Models;

public class DriverProfile
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Required, StringLength(30)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Range(0, 60)]
    public int YearsOfExperience { get; set; }

    [Range(1, 2000)]
    public decimal RatePerHour { get; set; } = 150m;

    public bool IsAvailable { get; set; } = true;

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime LocationUpdatedAt { get; set; } = DateTime.UtcNow;

    public double AverageRating { get; set; }
    public int RatingCount { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
