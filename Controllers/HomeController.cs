using System.Diagnostics;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using StallBazar.Models;
using StallBazar.Services;

namespace StallBazar.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public HomeController(IEmailSender emailSender, IConfiguration configuration)
        {
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var supportEmail = _configuration["SupportEmail"]
                ?? _configuration["Smtp:From"]
                ?? "stallbazar.support@gmail.com";
            var encoder = HtmlEncoder.Default;
            var message = $"<p><strong>From:</strong> {encoder.Encode(model.Name)} ({encoder.Encode(model.Email)})</p>" +
                          $"<p><strong>Topic:</strong> {encoder.Encode(model.Topic)}</p>" +
                          $"<p>{encoder.Encode(model.Message).Replace("\n", "<br />")}</p>";

            await _emailSender.SendEmailAsync(supportEmail, $"StallBazar support: {model.Topic}", message);
            TempData["Success"] = "Your inquiry has been sent. The StallBazar team will respond by email.";
            return RedirectToAction(nameof(Contact));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
