using System.ComponentModel.DataAnnotations;

namespace StallBazar.Models;

public class Event
{
    public int Id { get; set; }

    [Required, StringLength(140)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(220)]
    public string Venue { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Category { get; set; } = "Food";

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [StringLength(500)]
    public string? MapImageUrl { get; set; }

    [StringLength(160)]
    public string? MapHint { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime StartsAt { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime EndsAt { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? ApplicationDeadline { get; set; }

    [Range(1, 1000000)]
    public int? ExpectedFootfall { get; set; }

    [StringLength(160), EmailAddress]
    public string? ContactEmail { get; set; }

    [StringLength(40), Phone]
    public string? ContactPhone { get; set; }

    [StringLength(600)]
    public string? Facilities { get; set; }

    [StringLength(1200)]
    public string? VendorRequirements { get; set; }

    [StringLength(800)]
    public string? CancellationPolicy { get; set; }

    [Range(0, 999999)]
    public decimal PriceFrom { get; set; }

    public string OrganizerId { get; set; } = string.Empty;
    public ApplicationUser? Organizer { get; set; }

    public List<Stall> Stalls { get; set; } = [];
    public List<Booking> Bookings { get; set; } = [];
}
