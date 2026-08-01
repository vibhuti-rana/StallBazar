using System.ComponentModel.DataAnnotations;

namespace StallBazar.Models;

public class StallFormViewModel : IValidatableObject
{
    public int Id { get; set; }
    public int EventId { get; set; }

    [StringLength(30)]
    [Display(Name = "Stall number")]
    public string Number { get; set; } = string.Empty;

    [Range(1, 100)]
    [Display(Name = "Number of stalls")]
    public int Quantity { get; set; } = 8;

    [StringLength(8)]
    [Display(Name = "Number prefix")]
    public string NumberPrefix { get; set; } = "A";

    [Range(1, 999)]
    [Display(Name = "Start from")]
    public int StartingNumber { get; set; } = 1;

    [StringLength(80)]
    [Display(Name = "Stall name")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(30)]
    [Display(Name = "Stall category")]
    public string Tier { get; set; } = "Standard";

    [Required, StringLength(60)]
    [Display(Name = "Vendor type")]
    public string Type { get; set; } = "Food";

    [StringLength(80)]
    [Display(Name = "Map zone")]
    public string Zone { get; set; } = "Main aisle";

    [Required, StringLength(40)]
    [Display(Name = "Derived stall size")]
    public string Size { get; set; } = "3m x 3m";

    [Range(0.5, 9999)]
    [Display(Name = "Length")]
    public decimal Length { get; set; } = 3;

    [Range(0.5, 9999)]
    [Display(Name = "Breadth")]
    public decimal Breadth { get; set; } = 3;

    [Range(1, 999999)]
    [Display(Name = "Full stall price")]
    public decimal Price { get; set; } = 3000;

    [Display(Name = "Availability")]
    public StallStatus Status { get; set; } = StallStatus.Available;
    [Display(Name = "Map column")]
    [Range(0, 100)]
    public int PositionX { get; set; }
    [Display(Name = "Map row")]
    [Range(0, 100)]
    public int PositionY { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!StallCategories.All.Contains(Type, StringComparer.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("Select a valid stall type.", [nameof(Type)]);
        }

        if (!StallTiers.IsValid(Tier))
        {
            yield return new ValidationResult("Select a valid stall category.", [nameof(Tier)]);
        }

        if (Id == 0)
        {
            if (string.IsNullOrWhiteSpace(NumberPrefix))
            {
                yield return new ValidationResult("Enter a short prefix so generated stall numbers are easy to scan.", [nameof(NumberPrefix)]);
            }
        }
        else if (string.IsNullOrWhiteSpace(Number))
        {
            yield return new ValidationResult("Enter the stall number.", [nameof(Number)]);
        }
    }
}
