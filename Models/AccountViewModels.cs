using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StallBazar.Models;

public class LoginViewModel
{
    [Required, EmailAddress, GmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required, StringLength(120)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, GmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Vendor";
}

public class ResendVerificationViewModel
{
    [Required, EmailAddress, GmailAddress]
    [Display(Name = "Gmail address")]
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordViewModel
{
    [Required, EmailAddress, GmailAddress]
    [Display(Name = "Registered email address")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(8)]
    [Display(Name = "New password")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Confirm new password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ProfileSettingsViewModel
{
    [Required, StringLength(120)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, GmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [StringLength(120)]
    [Display(Name = "Business or organization")]
    public string? BusinessName { get; set; }

    [StringLength(80)]
    public string? City { get; set; }

    [StringLength(800)]
    [Display(Name = "Profile details")]
    public string? Bio { get; set; }

    public string? ExistingProfileImageUrl { get; set; }

    [Display(Name = "Profile image")]
    public IFormFile? ProfileImage { get; set; }
}

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(8)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
    [Display(Name = "Confirm new password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class GmailAddressAttribute : ValidationAttribute
{
    public GmailAddressAttribute()
    {
        ErrorMessage = "Use a Gmail address ending with @gmail.com. The account must also be verified by email.";
    }

    public override bool IsValid(object? value)
    {
        if (value is not string email || string.IsNullOrWhiteSpace(email))
        {
            return true;
        }

        return email.Trim().EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase);
    }
}
