using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StallBazar.Data;
using StallBazar.Models;

namespace StallBazar.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 10;
        page = Math.Max(1, page);
        var visibleCount = page * pageSize;
        var userId = _userManager.GetUserId(User)!;
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);
        var totalCount = await query.CountAsync();
        var notifications = await query
            .Take(visibleCount)
            .ToListAsync();

        foreach (var notification in notifications.Where(n => !n.IsRead))
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
        return View(new NotificationIndexViewModel
        {
            Notifications = notifications,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }
}
