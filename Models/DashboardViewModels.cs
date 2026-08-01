namespace StallBazar.Models;

public class AdminDashboardViewModel
{
    public int Users { get; set; }
    public int Events { get; set; }
    public int Stalls { get; set; }
    public int Bookings { get; set; }
    public List<Booking> RecentBookings { get; set; } = [];
    public List<ApplicationUser> RecentUsers { get; set; } = [];
}

public class OrganizerDashboardViewModel
{
    public ApplicationUser? Organizer { get; set; }
    public List<Event> Events { get; set; } = [];
    public List<Booking> PendingBookings { get; set; } = [];
}

public class VendorDashboardViewModel
{
    public ApplicationUser? Vendor { get; set; }
    public List<Event> UpcomingEvents { get; set; } = [];
    public List<Event> NearEvents { get; set; } = [];
    public List<Event> ComingSoonEvents { get; set; } = [];
    public List<Booking> MyBookings { get; set; } = [];
}
