using System.ComponentModel.DataAnnotations;

namespace BookDriver.ViewModels;

public class DriverDashboardViewModel
{
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    [Display(Name = "Driving license number")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Range(0, 60)]
    [Display(Name = "Years of experience")]
    public int YearsOfExperience { get; set; }

    [Range(1, 2000)]
    [Display(Name = "Rate per hour (₹)")]
    public decimal RatePerHour { get; set; }

    public bool IsAvailable { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime LocationUpdatedAt { get; set; }

    public int PendingRequestCount { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public bool HasActiveRide { get; set; }
}
