using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StallBazar.Data;
using StallBazar.Models;

namespace StallBazar.Controllers;

[Authorize(Roles = SeedData.OrganizerRole)]
public class StallsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StallsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Create(int eventId)
    {
        var ev = await FindOwnedEventAsync(eventId);
        if (ev is null)
        {
            return NotFound();
        }

        var existingStalls = await _context.Stalls.CountAsync(s => s.EventId == eventId);
        var starterType = StallCategories.All.Contains(ev.Category, StringComparer.OrdinalIgnoreCase)
            ? ev.Category
            : "Food";

        return View("Form", new StallFormViewModel
        {
            EventId = eventId,
            Type = starterType,
            Quantity = 8,
            NumberPrefix = "A",
            StartingNumber = existingStalls + 1,
            Price = ev.PriceFrom > 0 ? ev.PriceFrom : StallTiers.GetDefinition("Standard").DefaultPrice
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StallFormViewModel model)
    {
        if (!await OwnsEventAsync(model.EventId))
        {
            return NotFound();
        }

        ApplyCategoryDefaults(model);
        var stallNumbers = BuildStallNumbers(model);
        await ValidateUniqueNumbersAsync(model, stallNumbers);
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var stalls = stallNumbers.Select((number, index) => new Stall
        {
            EventId = model.EventId,
            Number = number,
            Name = BuildGeneratedName(model, number, stallNumbers.Count),
            Tier = model.Tier,
            Type = model.Type,
            Zone = model.Zone.Trim(),
            Size = model.Size,
            Length = model.Length,
            Breadth = model.Breadth,
            Price = model.Price,
            Status = model.Status,
            PositionX = model.PositionX + (index % 6),
            PositionY = model.PositionY + (index / 6)
        }).ToList();

        _context.Stalls.AddRange(stalls);
        await _context.SaveChangesAsync();

        TempData["Success"] = stalls.Count == 1
            ? $"Stall {stalls[0].Number} was created."
            : $"{stalls.Count} {model.Tier.ToLowerInvariant()} stalls were generated.";
        return RedirectToAction("Details", "Events", new { id = model.EventId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var stall = await FindOwnedStallAsync(id);
        if (stall is null)
        {
            return NotFound();
        }

        return View("Form", new StallFormViewModel
        {
            Id = stall.Id,
            EventId = stall.EventId,
            Number = stall.Number,
            Quantity = 1,
            NumberPrefix = string.Empty,
            StartingNumber = 1,
            Name = stall.Name,
            Tier = StallTiers.Normalize(stall.Tier),
            Type = stall.Type,
            Zone = stall.Zone,
            Size = StallTiers.GetDefinition(stall.Tier).Size,
            Length = StallTiers.GetDefinition(stall.Tier).Length,
            Breadth = StallTiers.GetDefinition(stall.Tier).Breadth,
            Price = stall.Price,
            Status = stall.Status,
            PositionX = stall.PositionX,
            PositionY = stall.PositionY
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StallFormViewModel model)
    {
        var stall = await FindOwnedStallAsync(model.Id);
        if (stall is null)
        {
            return NotFound();
        }

        model.EventId = stall.EventId;
        ApplyCategoryDefaults(model);
        await ValidateUniqueNumberAsync(model);
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        stall.Number = model.Number.Trim();
        stall.Name = model.Name.Trim();
        stall.Tier = model.Tier;
        stall.Type = model.Type;
        stall.Zone = model.Zone.Trim();
        stall.Size = model.Size;
        stall.Length = model.Length;
        stall.Breadth = model.Breadth;
        stall.Price = model.Price;
        stall.Status = model.Status;
        stall.PositionX = model.PositionX;
        stall.PositionY = model.PositionY;
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Events", new { id = stall.EventId });
    }

    private async Task<bool> OwnsEventAsync(int eventId)
    {
        var userId = _userManager.GetUserId(User)!;
        return await _context.Events.AnyAsync(e => e.Id == eventId && e.OrganizerId == userId);
    }

    private async Task ValidateUniqueNumberAsync(StallFormViewModel model)
    {
        model.Number = model.Number?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(model.Number))
        {
            return;
        }

        var normalizedNumber = model.Number.ToUpper();
        var duplicate = await _context.Stalls.AnyAsync(s =>
            s.EventId == model.EventId &&
            s.Id != model.Id &&
            s.Number.ToUpper() == normalizedNumber);
        if (duplicate)
        {
            ModelState.AddModelError(nameof(model.Number), "This stall number is already used for the event.");
        }
    }

    private async Task ValidateUniqueNumbersAsync(StallFormViewModel model, IReadOnlyCollection<string> stallNumbers)
    {
        if (stallNumbers.Count == 0)
        {
            ModelState.AddModelError(nameof(model.Quantity), "Generate at least one stall.");
            return;
        }

        var duplicateInBatch = stallNumbers
            .GroupBy(number => number, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateInBatch is not null)
        {
            ModelState.AddModelError(nameof(model.NumberPrefix), $"Generated stall number {duplicateInBatch.Key} appears more than once.");
            return;
        }

        var existingNumbers = await _context.Stalls
            .Where(s => s.EventId == model.EventId)
            .Select(s => s.Number)
            .ToListAsync();
        var duplicates = stallNumbers
            .Where(number => existingNumbers.Contains(number, StringComparer.OrdinalIgnoreCase))
            .Take(4)
            .ToList();
        if (duplicates.Count > 0)
        {
            ModelState.AddModelError(nameof(model.NumberPrefix), $"These stall numbers already exist: {string.Join(", ", duplicates)}.");
        }
    }

    private async Task<Event?> FindOwnedEventAsync(int eventId)
    {
        var userId = _userManager.GetUserId(User)!;
        return await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId && e.OrganizerId == userId);
    }

    private static IReadOnlyList<string> BuildStallNumbers(StallFormViewModel model)
    {
        var quantity = Math.Clamp(model.Quantity, 1, 100);
        if (quantity == 1 && !string.IsNullOrWhiteSpace(model.Number))
        {
            return [model.Number.Trim().ToUpperInvariant()];
        }

        var prefix = (model.NumberPrefix ?? string.Empty).Trim().ToUpperInvariant();
        return Enumerable.Range(model.StartingNumber, quantity)
            .Select(number => $"{prefix}{number:00}")
            .ToList();
    }

    private static string BuildGeneratedName(StallFormViewModel model, string number, int generatedCount)
    {
        if (!string.IsNullOrWhiteSpace(model.Name))
        {
            return generatedCount == 1 ? model.Name.Trim() : $"{model.Name.Trim()} {number}";
        }

        return $"{model.Tier} {model.Type} Stall";
    }

    private static void ApplyCategoryDefaults(StallFormViewModel model)
    {
        var definition = StallTiers.GetDefinition(model.Tier);
        model.Tier = definition.Name;
        model.Size = definition.Size;
        model.Length = definition.Length;
        model.Breadth = definition.Breadth;
        model.Type = string.IsNullOrWhiteSpace(model.Type) ? "Food" : model.Type.Trim();
        model.Zone = string.IsNullOrWhiteSpace(model.Zone) ? "Main aisle" : model.Zone.Trim();
        model.NumberPrefix = (model.NumberPrefix ?? string.Empty).Trim().ToUpperInvariant();
    }

    private async Task<Stall?> FindOwnedStallAsync(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        return await _context.Stalls
            .Include(s => s.Event)
            .FirstOrDefaultAsync(s => s.Id == id && s.Event!.OrganizerId == userId);
    }
}
