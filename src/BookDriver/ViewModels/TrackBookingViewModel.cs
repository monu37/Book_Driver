using BookDriver.Models;

namespace BookDriver.ViewModels;

public class TrackBookingViewModel
{
    public int BookingId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string DropoffAddress { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public double DriverLatitude { get; set; }
    public double DriverLongitude { get; set; }
    public double PickupLatitude { get; set; }
    public double PickupLongitude { get; set; }
}
