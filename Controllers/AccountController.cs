using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StallBazar.Data;
using StallBazar.Models;
using StallBazar.Services;

namespace StallBazar.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IWebHostEnvironment _environment;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IEmailSender emailSender, IWebHostEnvironment environment)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailSender = emailSender;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);
        var result = user is null
            ? Microsoft.AspNetCore.Identity.SignInResult.Failed
            : await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
        if (result.Succeeded)
        {
            return RedirectToLocal(returnUrl);
        }

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty, "Confirm your Gmail address before logging in.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var role = model.Role == SeedData.OrganizerRole ? SeedData.OrganizerRole : SeedData.VendorRole;
        var user = new ApplicationUser
        {
            FullName = model.FullName,
            Email = model.Email,
            UserName = model.Email,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, role);
            var confirmationLink = await SendVerificationEmailAsync(user);
            TempData["Success"] = "Account created. Check your Gmail verification link before logging in.";
            if (_environment.IsDevelopment())
            {
                TempData["DevVerificationLink"] = confirmationLink;
            }
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpGet]
    public IActionResult ResendVerification()
    {
        return View(new ResendVerificationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendVerification(ResendVerificationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is not null && !user.EmailConfirmed)
        {
            var confirmationLink = await SendVerificationEmailAsync(user);
            TempData["DevVerificationLink"] = confirmationLink;
        }

        TempData["Success"] = "If that Gmail is registered and unverified, a verification link has been sent.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is not null)
        {
            var resetLink = await SendPasswordResetEmailAsync(user);
            TempData["DevPasswordResetLink"] = resetLink;
        }

        TempData["Success"] = "If that Gmail is registered, a password reset link has been sent.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new ProfileSettingsViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            BusinessName = user.BusinessName,
            City = user.City,
            Bio = user.Bio,
            ExistingProfileImageUrl = user.ProfileImageUrl
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        user.FullName = model.FullName;
        user.PhoneNumber = model.PhoneNumber;
        user.BusinessName = model.BusinessName;
        user.City = model.City;
        user.Bio = model.Bio;
        var imageUrl = await SaveImageAsync(model.ProfileImage, "profiles");
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            user.ProfileImageUrl = imageUrl;
        }
        var emailChanged = !string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase);
        if (emailChanged)
        {
            user.Email = model.Email;
            user.UserName = model.Email;
            user.EmailConfirmed = false;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        if (emailChanged)
        {
            var confirmationLink = await SendVerificationEmailAsync(user);
            TempData["DevVerificationLink"] = confirmationLink;
        }

        TempData["Success"] = emailChanged
            ? "Profile updated. Verify your new Gmail before your next login."
            : "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Settings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new ProfileSettingsViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            BusinessName = user.BusinessName,
            City = user.City,
            Bio = user.Bio,
            ExistingProfileImageUrl = user.ProfileImageUrl
        });
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction(nameof(Login));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "Gmail verified. You can log in now."
            : "Email verification failed. Please register again or request a new verification email.";

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            TempData["Error"] = "Password reset link is invalid or expired.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View(new ResetPasswordViewModel { UserId = userId, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null)
        {
            TempData["Error"] = "Password reset link is invalid or expired.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        TempData["Success"] = "Password reset. You can log in now.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please check the password fields and try again.";
            return RedirectToAction(nameof(Settings));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "Password changed successfully."
            : string.Join(" ", result.Errors.Select(e => e.Description));

        return RedirectToAction(nameof(Settings));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    private async Task<string> SendVerificationEmailAsync(ApplicationUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, token }, Request.Scheme)!;
        await _emailSender.SendEmailAsync(user.Email!, "Confirm your StallBazar Gmail", $"Confirm your account: <a href=\"{confirmationLink}\">Verify Gmail</a>");
        return confirmationLink;
    }

    private async Task<string> SendPasswordResetEmailAsync(ApplicationUser user)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = Url.Action(nameof(ResetPassword), "Account", new { userId = user.Id, token }, Request.Scheme)!;
        await _emailSender.SendEmailAsync(user.Email!, "Reset your StallBazar password", $"Reset your password: <a href=\"{resetLink}\">Set a new password</a>");
        return resetLink;
    }

    private async Task<string?> SaveImageAsync(IFormFile? file, string folder)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
        {
            ModelState.AddModelError(string.Empty, "Upload a JPG, PNG, or WEBP image.");
            return null;
        }

        var uploadRoot = Path.Combine(_environment.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadRoot);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(uploadRoot, fileName);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);
        return $"/uploads/{folder}/{fileName}";
    }
}
