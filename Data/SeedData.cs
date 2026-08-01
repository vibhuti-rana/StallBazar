using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StallBazar.Models;

namespace StallBazar.Data;

public static class SeedData
{
    public const string AdminRole = "Admin";
    public const string OrganizerRole = "Organizer";
    public const string VendorRole = "Vendor";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();
        if (context.Database.IsSqlServer())
        {
            await EnsureSqlServerSchemaAsync(context);
        }
        else if (context.Database.IsSqlite())
        {
            await EnsureSqliteSchemaAsync(context);
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { AdminRole, OrganizerRole, VendorRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (!environment.IsDevelopment())
        {
            return;
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await EnsureUserAsync(userManager, "stallbazar.admin@gmail.com", "Admin User", "Admin@12345", AdminRole);
        var organizer = await EnsureUserAsync(userManager, "stallbazar.organizer@gmail.com", "Demo Organizer", "Organizer@12345", OrganizerRole);
        var vendor = await EnsureUserAsync(userManager, "stallbazar.vendor@gmail.com", "Demo Vendor", "Vendor@12345", VendorRole);

        if (!await context.Events.AnyAsync())
        {
            var sampleEvent = new Event
            {
                Name = "Kathmandu Artisan Market",
                Description = "A small-scale local event for food, craft, fashion, and community vendors.",
                Category = "Food",
                ImageUrl = "https://images.unsplash.com/photo-1501281668745-f7f57925c3b4?auto=format&fit=crop&w=1200&q=80",
                MapImageUrl = "https://images.unsplash.com/photo-1515169067865-5387ec356754?auto=format&fit=crop&w=1200&q=80",
                MapHint = "Main entrance at the north side, food court on the left, premium stalls near the stage.",
                Venue = "Bhrikutimandap Exhibition Hall",
                StartsAt = DateTime.Today.AddDays(14).AddHours(10),
                EndsAt = DateTime.Today.AddDays(14).AddHours(18),
                ApplicationDeadline = DateTime.Today.AddDays(11).AddHours(18),
                ExpectedFootfall = 2500,
                ContactEmail = organizer.Email,
                ContactPhone = organizer.PhoneNumber,
                Facilities = "Electricity access, shared Wi-Fi, security, waste collection, and vendor loading support.",
                VendorRequirements = "Bring your own display fixtures and arrive at least two hours before opening. Food vendors must provide valid hygiene documentation.",
                CancellationPolicy = "Cancellations made at least seven days before the event may be reviewed for a deposit transfer. Later cancellations are non-refundable.",
                PriceFrom = 2500,
                OrganizerId = organizer.Id
            };

            context.Events.Add(sampleEvent);
            await context.SaveChangesAsync();

            for (var i = 1; i <= 16; i++)
            {
                context.Stalls.Add(new Stall
                {
                    EventId = sampleEvent.Id,
                    Number = $"A{i:00}",
                    Name = i % 4 == 0 ? "Corner Showcase" : i % 3 == 0 ? "Food Counter" : "Vendor Stall",
                    Tier = i % 4 == 0 ? "Premium" : i % 2 == 0 ? "Standard" : "Basic",
                    Type = i % 5 == 0 ? "Books" : i % 4 == 0 ? "Accessories" : i % 3 == 0 ? "Clothes" : "Food",
                    Zone = i <= 4 ? "Front entrance" : i <= 8 ? "Food court" : i <= 12 ? "Center aisle" : "Stage side",
                    Size = i % 4 == 0 ? "4m x 4m" : "3m x 3m",
                    Length = i % 4 == 0 ? 4 : 3,
                    Breadth = i % 4 == 0 ? 4 : 3,
                    Price = i % 4 == 0 ? 4500 : 2500,
                    PositionX = (i - 1) % 4,
                    PositionY = (i - 1) / 4
                });
            }

            await context.SaveChangesAsync();
        }

        var legacyNormalStalls = await context.Stalls
            .Where(s => s.Tier == "Normal")
            .ToListAsync();
        foreach (var stall in legacyNormalStalls)
        {
            var definition = StallTiers.GetDefinition("Basic");
            stall.Tier = definition.Name;
            stall.Size = definition.Size;
            stall.Length = definition.Length;
            stall.Breadth = definition.Breadth;
        }
        if (legacyNormalStalls.Count > 0)
        {
            await context.SaveChangesAsync();
        }

        var demoEvent = await context.Events
            .FirstOrDefaultAsync(e => e.Name == "Kathmandu Artisan Market");
        if (demoEvent is not null)
        {
            demoEvent.OrganizerId = organizer.Id;
            if (demoEvent.EndsAt <= DateTime.Now)
            {
                demoEvent.StartsAt = DateTime.Today.AddDays(14).AddHours(10);
                demoEvent.EndsAt = DateTime.Today.AddDays(14).AddHours(18);
                demoEvent.ApplicationDeadline = DateTime.Today.AddDays(11).AddHours(18);
            }
            demoEvent.ExpectedFootfall ??= 2500;
            demoEvent.ContactEmail ??= organizer.Email;
            demoEvent.ContactPhone ??= organizer.PhoneNumber;
            demoEvent.Facilities ??= "Electricity access, shared Wi-Fi, security, waste collection, and vendor loading support.";
            demoEvent.VendorRequirements ??= "Bring your own display fixtures and arrive at least two hours before opening. Food vendors must provide valid hygiene documentation.";
            demoEvent.CancellationPolicy ??= "Cancellations made at least seven days before the event may be reviewed for a deposit transfer. Later cancellations are non-refundable.";
            await context.SaveChangesAsync();
        }

        _ = admin;
        _ = vendor;
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName
            };
            await userManager.CreateAsync(user, password);
        }
        else
        {
            user.UserName = email;
            user.Email = email;
            user.EmailConfirmed = true;
            user.FullName = string.IsNullOrWhiteSpace(user.FullName) ? fullName : user.FullName;
            await userManager.UpdateAsync(user);

            if (!await userManager.CheckPasswordAsync(user, password))
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, resetToken, password);
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        return user;
    }

    private static async Task EnsureSqlServerSchemaAsync(ApplicationDbContext context)
    {
        var commands = new[]
        {
            "IF COL_LENGTH('AspNetUsers', 'BusinessName') IS NULL ALTER TABLE AspNetUsers ADD BusinessName nvarchar(120) NULL",
            "IF COL_LENGTH('AspNetUsers', 'City') IS NULL ALTER TABLE AspNetUsers ADD City nvarchar(80) NULL",
            "IF COL_LENGTH('AspNetUsers', 'Bio') IS NULL ALTER TABLE AspNetUsers ADD Bio nvarchar(800) NULL",
            "IF COL_LENGTH('AspNetUsers', 'ProfileImageUrl') IS NULL ALTER TABLE AspNetUsers ADD ProfileImageUrl nvarchar(500) NULL",
            "IF COL_LENGTH('Events', 'Category') IS NULL ALTER TABLE Events ADD Category nvarchar(60) NOT NULL CONSTRAINT DF_Events_Category DEFAULT 'Food'",
            "IF COL_LENGTH('Events', 'ImageUrl') IS NULL ALTER TABLE Events ADD ImageUrl nvarchar(500) NULL",
            "IF COL_LENGTH('Events', 'MapImageUrl') IS NULL ALTER TABLE Events ADD MapImageUrl nvarchar(500) NULL",
            "IF COL_LENGTH('Events', 'MapHint') IS NULL ALTER TABLE Events ADD MapHint nvarchar(160) NULL",
            "IF COL_LENGTH('Events', 'ApplicationDeadline') IS NULL ALTER TABLE Events ADD ApplicationDeadline datetime2 NULL",
            "IF COL_LENGTH('Events', 'ExpectedFootfall') IS NULL ALTER TABLE Events ADD ExpectedFootfall int NULL",
            "IF COL_LENGTH('Events', 'ContactEmail') IS NULL ALTER TABLE Events ADD ContactEmail nvarchar(160) NULL",
            "IF COL_LENGTH('Events', 'ContactPhone') IS NULL ALTER TABLE Events ADD ContactPhone nvarchar(40) NULL",
            "IF COL_LENGTH('Events', 'Facilities') IS NULL ALTER TABLE Events ADD Facilities nvarchar(600) NULL",
            "IF COL_LENGTH('Events', 'VendorRequirements') IS NULL ALTER TABLE Events ADD VendorRequirements nvarchar(1200) NULL",
            "IF COL_LENGTH('Events', 'CancellationPolicy') IS NULL ALTER TABLE Events ADD CancellationPolicy nvarchar(800) NULL",
            "IF COL_LENGTH('Stalls', 'Name') IS NULL ALTER TABLE Stalls ADD Name nvarchar(80) NOT NULL CONSTRAINT DF_Stalls_Name DEFAULT ''",
            "IF COL_LENGTH('Stalls', 'Tier') IS NULL ALTER TABLE Stalls ADD Tier nvarchar(30) NOT NULL CONSTRAINT DF_Stalls_Tier DEFAULT 'Standard'",
            "IF COL_LENGTH('Stalls', 'Zone') IS NULL ALTER TABLE Stalls ADD Zone nvarchar(80) NOT NULL CONSTRAINT DF_Stalls_Zone DEFAULT 'Main aisle'",
            "IF COL_LENGTH('Stalls', 'Length') IS NULL ALTER TABLE Stalls ADD Length decimal(18,2) NOT NULL CONSTRAINT DF_Stalls_Length DEFAULT 3",
            "IF COL_LENGTH('Stalls', 'Breadth') IS NULL ALTER TABLE Stalls ADD Breadth decimal(18,2) NOT NULL CONSTRAINT DF_Stalls_Breadth DEFAULT 3",
            @"IF OBJECT_ID('Notifications', 'U') IS NULL CREATE TABLE Notifications (
                Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY,
                UserId nvarchar(450) NOT NULL,
                Title nvarchar(140) NOT NULL,
                Message nvarchar(800) NOT NULL,
                LinkUrl nvarchar(200) NULL,
                IsRead bit NOT NULL,
                CreatedAt datetime2 NOT NULL,
                CONSTRAINT FK_Notifications_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
            )"
        };

        foreach (var command in commands)
        {
            await context.Database.ExecuteSqlRawAsync(command);
        }
    }

    private static async Task EnsureSqliteSchemaAsync(ApplicationDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        var closeWhenDone = connection.State != System.Data.ConnectionState.Open;
        if (closeWhenDone)
        {
            await connection.OpenAsync();
        }

        try
        {
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info('Events')";
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    existingColumns.Add(reader.GetString(1));
                }
            }

            var additions = new Dictionary<string, string>
            {
                ["ApplicationDeadline"] = "ALTER TABLE Events ADD COLUMN ApplicationDeadline TEXT NULL",
                ["ExpectedFootfall"] = "ALTER TABLE Events ADD COLUMN ExpectedFootfall INTEGER NULL",
                ["ContactEmail"] = "ALTER TABLE Events ADD COLUMN ContactEmail TEXT NULL",
                ["ContactPhone"] = "ALTER TABLE Events ADD COLUMN ContactPhone TEXT NULL",
                ["Facilities"] = "ALTER TABLE Events ADD COLUMN Facilities TEXT NULL",
                ["VendorRequirements"] = "ALTER TABLE Events ADD COLUMN VendorRequirements TEXT NULL",
                ["CancellationPolicy"] = "ALTER TABLE Events ADD COLUMN CancellationPolicy TEXT NULL"
            };

            foreach (var addition in additions.Where(addition => !existingColumns.Contains(addition.Key)))
            {
                await context.Database.ExecuteSqlRawAsync(addition.Value);
            }
        }
        finally
        {
            if (closeWhenDone)
            {
                await connection.CloseAsync();
            }
        }
    }
}
