using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using StallBazar.Data;
using StallBazar.Models;
using StallBazar.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}

// Add services to the container.
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var useDevelopmentSqlServer = builder.Environment.IsDevelopment() && CanOpenSqlServer(defaultConnection);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        if (useDevelopmentSqlServer)
        {
            options.UseSqlServer(defaultConnection);
        }
        else
        {
            options.UseSqlite($"Data Source={Path.Combine(builder.Environment.ContentRootPath, "stallbazar-dev.db")}");
        }
    }
    else
    {
        options.UseSqlServer(defaultConnection);
    }
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")))
        .SetApplicationName("StallBazar");
}

var app = builder.Build();

await SeedData.InitializeAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();

static bool CanOpenSqlServer(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return false;
    }

    try
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            ConnectTimeout = 2
        };
        using var connection = new SqlConnection(builder.ConnectionString);
        connection.Open();
        return true;
    }
    catch
    {
        return false;
    }
}
