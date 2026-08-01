using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StallBazar.Data;
using StallBazar.Models;

namespace StallBazar.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole(SeedData.AdminRole))
        {
            return RedirectToAction(nameof(Admin));
        }

        if (User.IsInRole(SeedData.OrganizerRole))
        {
            return RedirectToAction(nameof(Organizer));
        }

        return RedirectToAction(nameof(Vendor));
    }

    [Authorize(Roles = SeedData.AdminRole)]
    public async Task<IActionResult> Admin()
    {
        var model = new AdminDashboardViewModel
        {
            Users = await _context.Users.CountAsync(),
            Events = await _context.Events.CountAsync(),
            Stalls = await _context.Stalls.CountAsync(),
            Bookings = await _context.Bookings.CountAsync(),
            RecentBookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Stall)
                .Include(b => b.Vendor)
                .OrderByDescending(b => b.RequestedAt)
                .Take(8)
                .ToListAsync(),
            RecentUsers = await _context.Users
                .OrderByDescending(u => u.Id)
                .Take(8)
                .ToListAsync()
        };

        return View(model);
    }

    [Authorize(Roles = SeedData.OrganizerRole)]
    public async Task<IActionResult> Organizer()
    {
        var userId = _userManager.GetUserId(User)!;
        var model = new OrganizerDashboardViewModel
        {
            Organizer = await _userManager.GetUserAsync(User),
            Events = await _context.Events
                .Include(e => e.Stalls)
                .Include(e => e.Bookings)
                .Where(e => e.OrganizerId == userId)
                .OrderBy(e => e.StartsAt)
                .ToListAsync(),
            PendingBookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Stall)
                .Include(b => b.Vendor)
                .Where(b => b.Event!.OrganizerId == userId && b.Status == BookingStatus.Pending)
                .OrderBy(b => b.RequestedAt)
                .ToListAsync()
        };

        return View(model);
    }

    [Authorize(Roles = SeedData.VendorRole)]
    public async Task<IActionResult> Vendor()
    {
        var userId = _userManager.GetUserId(User)!;
        var vendor = await _userManager.GetUserAsync(User);
        var vendorCity = string.IsNullOrWhiteSpace(vendor?.City) ? "Kathmandu" : vendor.City.Trim();
        var model = new VendorDashboardViewModel
        {
            Vendor = vendor,
            UpcomingEvents = await _context.Events
                .Include(e => e.Stalls)
                .Include(e => e.Organizer)
                .Where(e => e.EndsAt >= DateTime.Now &&
                            (!e.ApplicationDeadline.HasValue || e.ApplicationDeadline >= DateTime.Now))
                .OrderBy(e => e.StartsAt)
                .Take(6)
                .ToListAsync(),
            NearEvents = await _context.Events
                .Include(e => e.Stalls)
                .Include(e => e.Organizer)
                .Where(e => e.EndsAt >= DateTime.Now && e.Venue.Contains(vendorCity))
                .OrderBy(e => e.StartsAt)
                .Take(4)
                .ToListAsync(),
            ComingSoonEvents = await _context.Events
                .Include(e => e.Stalls)
                .Include(e => e.Organizer)
                .Where(e => e.StartsAt > DateTime.Now.AddDays(14))
                .OrderBy(e => e.StartsAt)
                .Take(4)
                .ToListAsync(),
            MyBookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Stall)
                .Where(b => b.VendorId == userId)
                .OrderByDescending(b => b.RequestedAt)
                .ToListAsync()
        };

        return View(model);
    }
}
