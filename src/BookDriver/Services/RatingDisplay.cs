namespace BookDriver.Services;

public static class RatingDisplay
{
    public static string Stars(double rating)
    {
        var rounded = (int)Math.Round(rating);
        return new string('★', Math.Clamp(rounded, 0, 5)) + new string('☆', 5 - Math.Clamp(rounded, 0, 5));
    }

    public static string Label(double rating, int count) =>
        count == 0 ? "No ratings yet" : $"{rating:0.0} ({count} ride{(count == 1 ? "" : "s")})";
}
