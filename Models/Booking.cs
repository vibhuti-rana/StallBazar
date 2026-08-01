using System.ComponentModel.DataAnnotations;

namespace StallBazar.Models;

public class Booking
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int StallId { get; set; }
    public Stall? Stall { get; set; }

    public string VendorId { get; set; } = string.Empty;
    public ApplicationUser? Vendor { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.NotSubmitted;

    [StringLength(500)]
    public string? PaymentReference { get; set; }

    [StringLength(800)]
    public string? VendorNote { get; set; }

    [StringLength(800)]
    public string? OrganizerNote { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedById { get; set; }
    public ApplicationUser? ReviewedBy { get; set; }
}

