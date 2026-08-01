using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StallBazar.Models;

namespace StallBazar.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Stall> Stalls => Set<Stall>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Event>()
            .HasOne(e => e.Organizer)
            .WithMany()
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Stall>()
            .HasOne(s => s.Event)
            .WithMany(e => e.Stalls)
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Booking>()
            .HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(b => b.Stall)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.StallId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(b => b.Vendor)
            .WithMany()
            .HasForeignKey(b => b.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(b => b.ReviewedBy)
            .WithMany()
            .HasForeignKey(b => b.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Stall>()
            .Property(s => s.RowVersion)
            .IsRowVersion();

        builder.Entity<Event>()
            .Property(e => e.PriceFrom)
            .HasPrecision(10, 2);

        builder.Entity<Stall>()
            .Property(s => s.Price)
            .HasPrecision(10, 2);

        builder.Entity<Stall>()
            .Property(s => s.Length)
            .HasPrecision(8, 2);

        builder.Entity<Stall>()
            .Property(s => s.Breadth)
            .HasPrecision(8, 2);
    }
}
