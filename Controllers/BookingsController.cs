using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StallBazar.Data;
using StallBazar.Models;
using StallBazar.Services;

namespace StallBazar.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public BookingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
    {
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [HttpPost]
    [Authorize(Roles = SeedData.VendorRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int stallId, string? vendorNote)
    {
        var stall = await _context.Stalls.Include(s => s.Event).FirstOrDefaultAsync(s => s.Id == stallId);
        if (stall is null)
        {
            return NotFound();
        }

        if (stall.Event is null ||
            stall.Event.EndsAt <= DateTime.Now ||
            (stall.Event.ApplicationDeadline.HasValue && stall.Event.ApplicationDeadline <= DateTime.Now))
        {
            TempData["Error"] = "Bookings are closed for this event.";
            return RedirectToAction("Details", "Events", new { id = stall.EventId });
        }

        vendorNote = vendorNote?.Trim();
        if (vendorNote?.Length > 800)
        {
            TempData["Error"] = "The vendor note must be 800 characters or fewer.";
            return RedirectToAction("Details", "Events", new { id = stall.EventId });
        }

        if (stall.Status != StallStatus.Available)
        {
            TempData["Error"] = "That stall is no longer available.";
            return RedirectToAction("Details", "Events", new { id = stall.EventId });
        }

        var userId = _userManager.GetUserId(User)!;
        var alreadyRequested = await _context.Bookings.AnyAsync(b =>
            b.StallId == stallId &&
            b.VendorId == userId &&
            b.Status != BookingStatus.Cancelled &&
            b.Status != BookingStatus.Rejected);

        if (alreadyRequested)
        {
            TempData["Error"] = "You already have an active request for this stall.";
            return RedirectToAction("Details", "Events", new { id = stall.EventId });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var claimed = await _context.Stalls
            .Where(s => s.Id == stallId && s.Status == StallStatus.Available)
            .ExecuteUpdateAsync(update => update.SetProperty(s => s.Status, StallStatus.Pending));
        if (claimed == 0)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Another vendor requested that stall first. Choose another available stall.";
            return RedirectToAction("Details", "Events", new { id = stall.EventId });
        }

        var booking = new Booking
        {
            EventId = stall.EventId,
            StallId = stall.Id,
            VendorId = userId,
            VendorNote = vendorNote,
            Status = BookingStatus.Pending
        };
        _context.Bookings.Add(booking);
        if (stall.Event?.OrganizerId is string organizerId)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = organizerId,
                Title = "New stall booking request",
                Message = $"A vendor requested stall {stall.Number} for {stall.Event.Name}.",
                LinkUrl = Url.Action("Organizer", "Dashboard")
            });
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["Success"] = "Booking request submitted. Add the 50% deposit reference from your vendor dashboard so the organizer can verify and approve it.";
        return RedirectToAction("Details", "Events", new { id = stall.EventId });
    }

    [Authorize(Roles = SeedData.OrganizerRole)]
    public async Task<IActionResult> Review(int id)
    {
        var booking = await FindOwnedBookingAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        return View(booking);
    }

    [HttpPost]
    [Authorize(Roles = SeedData.OrganizerRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? organizerNote)
    {
        var booking = await FindOwnedBookingAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.Status != BookingStatus.Pending)
        {
            TempData["Error"] = "This booking has already been reviewed.";
            return RedirectToAction(nameof(Review), new { id });
        }

        if (booking.PaymentStatus != PaymentStatus.Submitted && booking.PaymentStatus != PaymentStatus.Verified)
        {
            TempData["Error"] = "The vendor must submit the 50% deposit reference before this booking can be approved.";
            return RedirectToAction(nameof(Review), new { id });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var claimed = await _context.Stalls
            .Where(s => s.Id == booking.StallId &&
                        (s.Status == StallStatus.Available || s.Status == StallStatus.Pending))
            .ExecuteUpdateAsync(update => update.SetProperty(s => s.Status, StallStatus.Booked));
        if (claimed == 0)
        {
            booking.Status = BookingStatus.Rejected;
            booking.OrganizerNote = "Automatically rejected because the stall was no longer available.";
            booking.ReviewedAt = DateTime.UtcNow;
            booking.ReviewedById = _userManager.GetUserId(User);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Error"] = "The stall was already booked, so this request was rejected.";
            return RedirectToAction("Organizer", "Dashboard");
        }

        booking.Status = BookingStatus.Approved;
        booking.PaymentStatus = PaymentStatus.Verified;
        booking.OrganizerNote = organizerNote;
        booking.ReviewedAt = DateTime.UtcNow;
        booking.ReviewedById = _userManager.GetUserId(User);

        var competingRequests = await _context.Bookings
            .Where(b => b.StallId == booking.StallId && b.Id != booking.Id && b.Status == BookingStatus.Pending)
            .ToListAsync();
        foreach (var request in competingRequests)
        {
            request.Status = BookingStatus.Rejected;
            request.OrganizerNote = "Rejected because another request for this stall was approved.";
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedById = _userManager.GetUserId(User);
        }

        try
        {
            _context.Notifications.Add(new Notification
            {
                UserId = booking.VendorId,
                Title = "Stall booking approved",
                Message = $"Your booking for stall {booking.Stall?.Number} at {booking.Event?.Name} has been approved.",
                LinkUrl = Url.Action("Vendor", "Dashboard")
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            if (!string.IsNullOrWhiteSpace(booking.Vendor?.Email))
            {
                await _emailSender.SendEmailAsync(
                    booking.Vendor.Email,
                    "Your StallBazar booking was approved",
                    $"Your booking for stall {booking.Stall?.Number} at {booking.Event?.Name} has been approved.");
            }
            TempData["Success"] = "Booking approved and stall marked as booked.";
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Another update changed this stall first. Please review the latest availability.";
        }

        return RedirectToAction("Organizer", "Dashboard");
    }

    [HttpPost]
    [Authorize(Roles = SeedData.OrganizerRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? organizerNote)
    {
        var booking = await FindOwnedBookingAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.Status != BookingStatus.Pending)
        {
            TempData["Error"] = "Only pending bookings can be rejected.";
            return RedirectToAction(nameof(Review), new { id });
        }

        booking.Status = BookingStatus.Rejected;
        if (booking.Stall is not null && booking.Stall.Status == StallStatus.Pending)
        {
            booking.Stall.Status = StallStatus.Available;
        }
        booking.OrganizerNote = organizerNote;
        booking.ReviewedAt = DateTime.UtcNow;
        booking.ReviewedById = _userManager.GetUserId(User);
        await _context.SaveChangesAsync();
        _context.Notifications.Add(new Notification
        {
            UserId = booking.VendorId,
            Title = "Stall booking rejected",
            Message = $"Your booking request for stall {booking.Stall?.Number} was rejected.",
            LinkUrl = Url.Action("Vendor", "Dashboard")
        });
        await _context.SaveChangesAsync();
        TempData["Success"] = "Booking rejected.";

        return RedirectToAction("Organizer", "Dashboard");
    }

    [HttpPost]
    [Authorize(Roles = SeedData.VendorRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePayment(int id, string paymentReference)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.VendorId == userId);
        if (booking is null)
        {
            return NotFound();
        }

        paymentReference = paymentReference?.Trim() ?? string.Empty;
        if (booking.Status != BookingStatus.Pending)
        {
            TempData["Error"] = "Payment details can only be changed while the booking is pending.";
            return RedirectToAction("Vendor", "Dashboard");
        }
        if (booking.PaymentStatus == PaymentStatus.Verified)
        {
            TempData["Error"] = "The payment has already been verified and cannot be changed.";
            return RedirectToAction("Vendor", "Dashboard");
        }
        if (string.IsNullOrWhiteSpace(paymentReference) || paymentReference.Length > 500)
        {
            TempData["Error"] = "Enter a valid payment reference of 500 characters or fewer.";
            return RedirectToAction("Vendor", "Dashboard");
        }

        booking.PaymentReference = paymentReference;
        booking.PaymentStatus = PaymentStatus.Submitted;
        var organizerId = await _context.Events
            .Where(e => e.Id == booking.EventId)
            .Select(e => e.OrganizerId)
            .FirstAsync();
        _context.Notifications.Add(new Notification
        {
            UserId = organizerId,
            Title = "Deposit reference submitted",
            Message = "A vendor submitted a deposit reference for booking review.",
            LinkUrl = Url.Action(nameof(Review), "Bookings", new { id = booking.Id })
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "Payment reference submitted for organizer review.";
        return RedirectToAction("Vendor", "Dashboard");
    }

    [HttpPost]
    [Authorize(Roles = SeedData.OrganizerRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePaymentStatus(int id, PaymentStatus paymentStatus)
    {
        var booking = await FindOwnedBookingAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.Status != BookingStatus.Pending)
        {
            TempData["Error"] = "Payment status cannot be changed after the booking is reviewed.";
            return RedirectToAction(nameof(Review), new { id });
        }
        if (paymentStatus is PaymentStatus.NotSubmitted ||
            (paymentStatus == PaymentStatus.Verified && string.IsNullOrWhiteSpace(booking.PaymentReference)))
        {
            TempData["Error"] = "A submitted payment reference is required before verification.";
            return RedirectToAction(nameof(Review), new { id });
        }

        booking.PaymentStatus = paymentStatus;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Payment status updated.";

        return RedirectToAction(nameof(Review), new { id });
    }

    [HttpPost]
    [Authorize(Roles = SeedData.VendorRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _context.Bookings
            .Include(b => b.Event)
            .Include(b => b.Stall)
            .FirstOrDefaultAsync(b => b.Id == id && b.VendorId == userId);
        if (booking is null)
        {
            return NotFound();
        }
        if (booking.Status != BookingStatus.Pending)
        {
            TempData["Error"] = "Only pending booking requests can be cancelled.";
            return RedirectToAction("Vendor", "Dashboard");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.ReviewedAt = DateTime.UtcNow;
        if (booking.Stall?.Status == StallStatus.Pending)
        {
            booking.Stall.Status = StallStatus.Available;
        }
        if (booking.Event is not null)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = booking.Event.OrganizerId,
                Title = "Booking request cancelled",
                Message = $"A vendor cancelled the request for stall {booking.Stall?.Number} at {booking.Event.Name}.",
                LinkUrl = Url.Action("Organizer", "Dashboard")
            });
        }
        await _context.SaveChangesAsync();
        TempData["Success"] = "Your pending booking request was cancelled.";
        return RedirectToAction("Vendor", "Dashboard");
    }

    private async Task<Booking?> FindOwnedBookingAsync(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        return await _context.Bookings
            .Include(b => b.Event)
            .Include(b => b.Stall)
            .Include(b => b.Vendor)
            .FirstOrDefaultAsync(b => b.Id == id && b.Event!.OrganizerId == userId);
    }
}
