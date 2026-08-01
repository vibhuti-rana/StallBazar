using System.ComponentModel.DataAnnotations;

namespace StallBazar.Models;

public class Notification
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Required, StringLength(140)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(800)]
    public string Message { get; set; } = string.Empty;

    [StringLength(200)]
    public string? LinkUrl { get; set; }

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
