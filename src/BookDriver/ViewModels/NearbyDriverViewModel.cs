namespace BookDriver.ViewModels;

public class NearbyDriverViewModel
{
    public int DriverProfileId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string VehiclePlateNumber { get; set; } = string.Empty;
    public decimal RatePerKm { get; set; }
    public double DistanceKm { get; set; }
}

public class NearbySearchViewModel
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusKm { get; set; } = 10;
    public List<NearbyDriverViewModel> Drivers { get; set; } = new();
}
