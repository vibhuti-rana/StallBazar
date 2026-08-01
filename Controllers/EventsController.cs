using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StallBazar.Data;
using StallBazar.Models;

namespace StallBazar.Controllers;

[Authorize]
public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public EventsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? q = null, string? category = null)
    {
        var query = _context.Events
            .Include(e => e.Stalls)
            .Include(e => e.Organizer)
            .Where(e => e.EndsAt >= DateTime.Now)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(e =>
                e.Name.Contains(q) ||
                e.Venue.Contains(q) ||
                e.Description.Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(e => e.Category == category);
        }

        var events = await query.OrderBy(e => e.StartsAt).ToListAsync();
        ViewData["EventSearch"] = q;
        ViewData["EventCategory"] = category;
        return View(events);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var ev = await _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Stalls.OrderBy(s => s.PositionY).ThenBy(s => s.PositionX))
            .ThenInclude(s => s.Bookings)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev is null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        var model = new EventDetailsViewModel
        {
            Event = ev,
            IsOrganizerOwner = userId is not null && ev.OrganizerId == userId,
            IsVendor = User.IsInRole(SeedData.VendorRole),
            CurrentVendorPendingStallId = userId is null
                ? null
                : await _context.Bookings
                    .Where(b => b.EventId == id && b.VendorId == userId && b.Status == BookingStatus.Pending)
                    .Select(b => (int?)b.StallId)
                    .FirstOrDefaultAsync()
        };

        return View(model);
    }

    [Authorize(Roles = SeedData.OrganizerRole)]
    public async Task<IActionResult> Create()
    {
        var organizer = await _userManager.GetUserAsync(User);
        return View("Form", new EventFormViewModel
        {
            ContactEmail = organizer?.Email ?? string.Empty,
            ContactPhone = organizer?.PhoneNumber
        });
    }

    [HttpPost]
    [Authorize(Roles = SeedData.OrganizerRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        ValidateEventForm(model);
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var eventImageUrl = await SaveUploadAsync(model.EventImage, "events", "Event photo", EventImageExtensions);
        var mapImageUrl = await SaveUploadAsync(model.MapImage, "maps", "Stall layout", MapLayoutExtensions);
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var ev = new Event
        {
            Name = model.Name,
            Venue = model.Venue,
            Description = model.Description,
            Category = model.Category,
            MapHint = model.MapHint,
            ImageUrl = eventImageUrl,
            MapImageUrl = mapImageUrl,
            StartsAt = model.StartsAt,
            EndsAt = model.EndsAt,
            ApplicationDeadline = model.ApplicationDeadline,
            ExpectedFootfall = model.ExpectedFootfall,
            ContactEmail = model.ContactEmail.Trim(),
            ContactPhone = model.ContactPhone?.Trim(),
            Facilities = model.Facilities?.Trim(),
            VendorRequirements = model.VendorRequirements?.Trim(),
            CancellationPolicy = model.CancellationPolicy?.Trim(),
            PriceFrom = model.PriceFrom,
            OrganizerId = _userManager.GetUserId(User)!
        };

        _context.Events.Add(ev);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Event profile saved. Add your stall inventory and layout positions next.";
        return RedirectToAction("Create", "Stalls", new { eventId = ev.Id });
    }

    [Authorize(Roles = SeedData.OrganizerRole)]
    public async Task<IActionResult> Edit(int id)
    {
        var ev = await FindOwnedEventAsync(id);
        if (ev is null)
        {
            return NotFound();
        }

        return View("Form", new EventFormViewModel
        {
            Id = ev.Id,
            Name = ev.Name,
            Venue = ev.Venue,
            Description = ev.Description,
            Category = ev.Category,
            MapHint = ev.MapHint,
            ExistingImageUrl = ev.ImageUrl,
            ExistingMapImageUrl = ev.MapImageUrl,
            StartsAt = ev.StartsAt,
            EndsAt = ev.EndsAt,
            ApplicationDeadline = ev.ApplicationDeadline,
            ExpectedFootfall = ev.ExpectedFootfall,
            ContactEmail = ev.ContactEmail ?? ev.Organizer?.Email ?? string.Empty,
            ContactPhone = ev.ContactPhone ?? ev.Organizer?.PhoneNumber,
            Facilities = ev.Facilities,
            VendorRequirements = ev.VendorRequirements,
            CancellationPolicy = ev.CancellationPolicy,
            PriceFrom = ev.PriceFrom
        });
    }

    [HttpPost]
    [Authorize(Roles = SeedData.OrganizerRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventFormViewModel model)
    {
        ValidateEventForm(model);
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var ev = await FindOwnedEventAsync(model.Id);
        if (ev is null)
        {
            return NotFound();
        }

        ev.Name = model.Name;
        ev.Venue = model.Venue;
        ev.Description = model.Description;
        ev.Category = model.Category;
        ev.MapHint = model.MapHint;
        var imageUrl = await SaveUploadAsync(model.EventImage, "events", "Event photo", EventImageExtensions);
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            ev.ImageUrl = imageUrl;
        }
        var mapImageUrl = await SaveUploadAsync(model.MapImage, "maps", "Stall layout", MapLayoutExtensions);
        if (!string.IsNullOrWhiteSpace(mapImageUrl))
        {
            ev.MapImageUrl = mapImageUrl;
        }
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }
        ev.StartsAt = model.StartsAt;
        ev.EndsAt = model.EndsAt;
        ev.ApplicationDeadline = model.ApplicationDeadline;
        ev.ExpectedFootfall = model.ExpectedFootfall;
        ev.ContactEmail = model.ContactEmail.Trim();
        ev.ContactPhone = model.ContactPhone?.Trim();
        ev.Facilities = model.Facilities?.Trim();
        ev.VendorRequirements = model.VendorRequirements?.Trim();
        ev.CancellationPolicy = model.CancellationPolicy?.Trim();
        ev.PriceFrom = model.PriceFrom;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = ev.Id });
    }

    private async Task<Event?> FindOwnedEventAsync(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        return await _context.Events
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerId == userId);
    }

    private void ValidateEventForm(EventFormViewModel model)
    {
        ValidateUpload(model.EventImage, "Event photo", EventImageContentTypes, "JPG, PNG, or WEBP image");
        ValidateUpload(model.MapImage, "Stall layout", MapLayoutContentTypes, "JPG, PNG, WEBP, or PDF file");
    }

    private static readonly string[] EventImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] MapLayoutExtensions = [".jpg", ".jpeg", ".png", ".webp", ".pdf"];
    private static readonly string[] EventImageContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private static readonly string[] MapLayoutContentTypes = ["image/jpeg", "image/png", "image/webp", "application/pdf"];

    private void ValidateUpload(IFormFile? file, string label, string[] allowedTypes, string allowedDescription)
    {
        if (file is null || file.Length == 0)
        {
            return;
        }

        const long maximumBytes = 5 * 1024 * 1024;
        if (file.Length > maximumBytes)
        {
            ModelState.AddModelError(string.Empty, $"{label} must be 5 MB or smaller.");
        }
        if (!allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, $"{label} must be a {allowedDescription}.");
        }
    }

    private async Task<string?> SaveUploadAsync(IFormFile? file, string folder, string label, string[] allowedExtensions)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, $"{label} has an unsupported file extension.");
            return null;
        }

        var uploadRoot = Path.Combine(_environment.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadRoot);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(uploadRoot, fileName);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);
        return $"/uploads/{folder}/{fileName}";
    }
}
