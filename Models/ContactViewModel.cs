using System.ComponentModel.DataAnnotations;

namespace StallBazar.Models;

public class ContactViewModel
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Topic { get; set; } = "General support";

    [Required, StringLength(1500, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;
}
