namespace StallBazar.Models;

public class NotificationIndexViewModel
{
    public List<Notification> Notifications { get; set; } = [];
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }

    public int VisibleCount => Notifications.Count;
    public bool HasMore => VisibleCount < TotalCount;
}
