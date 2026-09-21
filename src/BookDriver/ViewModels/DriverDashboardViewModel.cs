using System.ComponentModel.DataAnnotations;

namespace BookDriver.ViewModels;

public class DriverDashboardViewModel
{
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Vehicle model")]
    public string VehicleModel { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Display(Name = "Vehicle plate number")]
    public string VehiclePlateNumber { get; set; } = string.Empty;

    [Range(1, 500)]
    [Display(Name = "Rate per km (₹)")]
    public decimal RatePerKm { get; set; }

    public bool IsAvailable { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime LocationUpdatedAt { get; set; }

    public int PendingRequestCount { get; set; }
}
