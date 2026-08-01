using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using StallBazar.Data;
using StallBazar.Models;
using StallBazar.Services;

var builder = WebApplication.CreateBuilder(args);
var railwayPort = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(railwayPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

if (builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}

// Add services to the container.
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var useDevelopmentSqlServer = builder.Environment.IsDevelopment()
    && HasUsableSqlServerConnection(defaultConnection)
    && CanOpenSqlServer(defaultConnection);
var useProductionSqlServer = !builder.Environment.IsDevelopment()
    && HasUsableSqlServerConnection(defaultConnection);

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
        if (useProductionSqlServer)
        {
            options.UseSqlServer(defaultConnection);
        }
        else
        {
            options.UseSqlite($"Data Source={GetSqliteDatabasePath(builder.Environment, builder.Configuration)}");
        }
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
if (!string.IsNullOrWhiteSpace(railwayPort))
{
    app.UseForwardedHeaders();
}

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

static string GetSqliteDatabasePath(IHostEnvironment environment, IConfiguration configuration)
{
    var configuredPath = configuration["Sqlite:DatabasePath"];
    var databasePath = !string.IsNullOrWhiteSpace(configuredPath)
        ? configuredPath
        : Path.Combine(environment.ContentRootPath, "stallbazar.db");

    if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT")) ||
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_PROJECT_ID")))
    {
        databasePath = !string.IsNullOrWhiteSpace(configuredPath)
            ? configuredPath
            : "/data/stallbazar.db";
    }

    var directory = Path.GetDirectoryName(databasePath);
    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }

    return databasePath;
}

static bool HasUsableSqlServerConnection(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return false;
    }

    return !connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase);
}

static bool CanOpenSqlServer(string? connectionString)
{
    if (!HasUsableSqlServerConnection(connectionString))
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
