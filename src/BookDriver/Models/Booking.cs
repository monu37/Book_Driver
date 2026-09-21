using System.ComponentModel.DataAnnotations;

namespace BookDriver.Models;

public class Booking
{
    public int Id { get; set; }

    [Required]
    public string CustomerId { get; set; } = string.Empty;
    public ApplicationUser? Customer { get; set; }

    public int DriverProfileId { get; set; }
    public DriverProfile? DriverProfile { get; set; }

    [Required, StringLength(250)]
    public string PickupAddress { get; set; } = string.Empty;
    public double PickupLatitude { get; set; }
    public double PickupLongitude { get; set; }

    [Required, StringLength(250)]
    public string DropoffAddress { get; set; } = string.Empty;

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public double DistanceKm { get; set; }
    public decimal EstimatedFare { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
