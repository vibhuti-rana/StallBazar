using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StallBazar.Models;

public class EventFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required, StringLength(140)]
    [Display(Name = "Event name")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(220)]
    [Display(Name = "Venue")]
    public string Venue { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    [Display(Name = "Event description")]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(60)]
    [Display(Name = "Event category")]
    public string Category { get; set; } = "Food";

    [StringLength(160)]
    [Display(Name = "Map location guide")]
    public string? MapHint { get; set; }

    public string? ExistingImageUrl { get; set; }

    [Display(Name = "Event photo")]
    public IFormFile? EventImage { get; set; }

    public string? ExistingMapImageUrl { get; set; }

    [Display(Name = "Stall layout file")]
    public IFormFile? MapImage { get; set; }

    [Required]
    [Display(Name = "Starts at")]
    [DataType(DataType.DateTime)]
    public DateTime StartsAt { get; set; } = DateTime.Today.AddDays(7).AddHours(10);

    [Required]
    [Display(Name = "Ends at")]
    [DataType(DataType.DateTime)]
    public DateTime EndsAt { get; set; } = DateTime.Today.AddDays(7).AddHours(18);

    [Display(Name = "Vendor application deadline")]
    [DataType(DataType.DateTime)]
    public DateTime? ApplicationDeadline { get; set; } = DateTime.Today.AddDays(5).AddHours(18);

    [Range(1, 1000000)]
    [Display(Name = "Expected visitors")]
    public int? ExpectedFootfall { get; set; }

    [Required, EmailAddress, StringLength(160)]
    [Display(Name = "Vendor contact email")]
    public string ContactEmail { get; set; } = string.Empty;

    [Phone, StringLength(40)]
    [Display(Name = "Vendor contact phone")]
    public string? ContactPhone { get; set; }

    [StringLength(600)]
    [Display(Name = "Facilities included")]
    public string? Facilities { get; set; }

    [StringLength(1200)]
    [Display(Name = "Vendor requirements")]
    public string? VendorRequirements { get; set; }

    [StringLength(800)]
    [Display(Name = "Cancellation and refund policy")]
    public string? CancellationPolicy { get; set; }

    [Range(0, 999999)]
    [Display(Name = "Starting stall price")]
    public decimal PriceFrom { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndsAt <= StartsAt)
        {
            yield return new ValidationResult(
                "The event end time must be later than its start time.",
                [nameof(EndsAt)]);
        }

        if (ApplicationDeadline.HasValue && ApplicationDeadline.Value > StartsAt)
        {
            yield return new ValidationResult(
                "The vendor application deadline must be before the event starts.",
                [nameof(ApplicationDeadline)]);
        }

        if (!EventCategories.All.Contains(Category, StringComparer.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("Select a valid event category.", [nameof(Category)]);
        }
    }
}
