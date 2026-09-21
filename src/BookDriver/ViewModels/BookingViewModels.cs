using System.ComponentModel.DataAnnotations;
using BookDriver.Models;

namespace BookDriver.ViewModels;

public class CreateBookingViewModel
{
    public int DriverProfileId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public decimal RatePerKm { get; set; }
    public double DistanceKm { get; set; }
    public decimal EstimatedFare { get; set; }

    public double PickupLatitude { get; set; }
    public double PickupLongitude { get; set; }

    [Required, StringLength(250)]
    [Display(Name = "Pickup address")]
    public string PickupAddress { get; set; } = string.Empty;

    [Required, StringLength(250)]
    [Display(Name = "Drop-off address")]
    public string DropoffAddress { get; set; } = string.Empty;
}

public class BookingListItemViewModel
{
    public int Id { get; set; }
    public string OtherPartyName { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string DropoffAddress { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public double DistanceKm { get; set; }
    public decimal EstimatedFare { get; set; }
    public DateTime RequestedAt { get; set; }
}
