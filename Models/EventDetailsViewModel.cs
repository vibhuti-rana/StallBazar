namespace StallBazar.Models;

public class EventDetailsViewModel
{
    public Event Event { get; set; } = new();
    public bool IsOrganizerOwner { get; set; }
    public bool IsVendor { get; set; }
    public int? CurrentVendorPendingStallId { get; set; }
}
