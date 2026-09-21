using System.ComponentModel.DataAnnotations;
using BookDriver.Models;

namespace BookDriver.ViewModels;

public class CreateBookingViewModel
{
    public int DriverProfileId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public decimal RatePerHour { get; set; }
    public double DistanceKm { get; set; }
    public decimal EstimatedFare { get; set; }

    public double PickupLatitude { get; set; }
    public double PickupLongitude { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Your car's model")]
    public string CarModel { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Display(Name = "Your car's registration number")]
    public string CarNumber { get; set; } = string.Empty;

    [Required, StringLength(250)]
    [Display(Name = "Pickup address")]
    public string PickupAddress { get; set; } = string.Empty;

    [Required, StringLength(250)]
    [Display(Name = "Drop-off address")]
    public string DropoffAddress { get; set; } = string.Empty;

    [Required]
    [Display(Name = "When do you need the driver?")]
    [DataType(DataType.DateTime)]
    public DateTime TripStartAt { get; set; } = DateTime.Now.AddMinutes(30);

    [Range(1, 24)]
    [Display(Name = "How many hours do you need the driver for?")]
    public double EstimatedHours { get; set; } = 2;
}

public class BookingListItemViewModel
{
    public int Id { get; set; }
    public string OtherPartyName { get; set; } = string.Empty;
    public string CarModel { get; set; } = string.Empty;
    public string CarNumber { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string DropoffAddress { get; set; } = string.Empty;
    public DateTime TripStartAt { get; set; }
    public double EstimatedHours { get; set; }
    public BookingStatus Status { get; set; }
    public double DistanceKm { get; set; }
    public decimal EstimatedFare { get; set; }
    public DateTime RequestedAt { get; set; }
}
