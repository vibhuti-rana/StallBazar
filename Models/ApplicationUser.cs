using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace StallBazar.Models;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(120)]
    public string? BusinessName { get; set; }

    [StringLength(80)]
    public string? City { get; set; }

    [StringLength(800)]
    public string? Bio { get; set; }

    [StringLength(500)]
    public string? ProfileImageUrl { get; set; }
}
