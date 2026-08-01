namespace StallBazar.Models;

public enum StallStatus
{
    Available,
    Pending,
    Booked,
    Unavailable
}

public enum BookingStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public enum PaymentStatus
{
    NotSubmitted,
    Submitted,
    Verified,
    Rejected
}

public static class EventCategories
{
    public static readonly string[] All =
    [
        "Food",
        "Concert",
        "Fashion",
        "Books",
        "Games",
        "Accessories",
        "Culture",
        "Technology",
        "Community"
    ];
}

public static class StallCategories
{
    public static readonly string[] All =
    [
        "Food",
        "Clothes",
        "Accessories",
        "Games",
        "Books",
        "Art & Craft",
        "Electronics",
        "Beauty",
        "General"
    ];
}

public static class StallTiers
{
    public static readonly string[] All =
    [
        "Basic",
        "Standard",
        "Premium"
    ];

    public static bool IsValid(string? tier)
    {
        return All.Contains(tier ?? string.Empty, StringComparer.OrdinalIgnoreCase) ||
            string.Equals(tier, "Normal", StringComparison.OrdinalIgnoreCase);
    }

    public static string Normalize(string? tier)
    {
        return string.Equals(tier, "Normal", StringComparison.OrdinalIgnoreCase)
            ? "Basic"
            : All.FirstOrDefault(t => string.Equals(t, tier, StringComparison.OrdinalIgnoreCase)) ?? "Standard";
    }

    public static StallTierDefinition GetDefinition(string? tier)
    {
        return Normalize(tier) switch
        {
            "Basic" => new StallTierDefinition("Basic", "2m x 2m", 2, 2, 2000, "Compact booth for new vendors, books, accessories, or low-inventory displays."),
            "Premium" => new StallTierDefinition("Premium", "4m x 4m", 4, 4, 5000, "Larger high-visibility stall for anchor vendors, corner displays, or food counters."),
            _ => new StallTierDefinition("Standard", "3m x 3m", 3, 3, 3000, "Balanced stall size for most vendors and general event layouts.")
        };
    }
}

public record StallTierDefinition(
    string Name,
    string Size,
    decimal Length,
    decimal Breadth,
    decimal DefaultPrice,
    string Description);
