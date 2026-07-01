using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Common;

namespace Infrastructure.Data;

public class AppDbContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<WorkerProfile> WorkerProfiles => Set<WorkerProfile>();
    public DbSet<WorkerService> WorkerServices => Set<WorkerService>();
    public DbSet<ServiceImage> ServiceImages => Set<ServiceImage>();
    public DbSet<WorkerAvailability> WorkerAvailabilities => Set<WorkerAvailability>();
    public DbSet<WorkerPortfolioItem> WorkerPortfolioItems => Set<WorkerPortfolioItem>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityUser<int>>().ToTable("Users");
        builder.Entity<IdentityRole<int>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");

        builder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.SeoUrl).IsUnique().HasFilter("[SeoUrl] IS NOT NULL");
            e.HasIndex(c => c.SortOrder);
            e.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(c => !c.IsDeleted);
        });

        builder.Entity<WorkerProfile>(e =>
        {
            e.HasIndex(w => w.UserId).IsUnique();
            e.Property(w => w.HourlyRate).HasPrecision(18, 2);
            e.Property(w => w.StartingPrice).HasPrecision(18, 2);
            e.Property(w => w.MinimumJobValue).HasPrecision(18, 2);
            e.HasQueryFilter(w => !w.IsDeleted);
        });

        builder.Entity<WorkerService>(e =>
        {
            e.HasOne(s => s.WorkerProfile)
                .WithMany()
                .HasForeignKey(s => s.WorkerProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(s => s.Price).HasPrecision(18, 2);
            e.HasQueryFilter(s => !s.IsDeleted);
        });

        builder.Entity<ServiceImage>(e =>
        {
            e.HasOne(i => i.WorkerService)
                .WithMany(s => s.Images)
                .HasForeignKey(i => i.WorkerServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WorkerAvailability>(e =>
        {
            e.HasOne(a => a.WorkerProfile)
                .WithMany()
                .HasForeignKey(a => a.WorkerProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WorkerPortfolioItem>(e =>
        {
            e.HasOne(p => p.WorkerProfile)
                .WithMany()
                .HasForeignKey(p => p.WorkerProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Booking>(e =>
        {
            e.HasOne(b => b.WorkerProfile)
                .WithMany()
                .HasForeignKey(b => b.WorkerProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.WorkerService)
                .WithMany()
                .HasForeignKey(b => b.WorkerServiceId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(b => b.TotalPrice).HasPrecision(18, 2);
            e.Property(b => b.CommissionAmount).HasPrecision(18, 2);
            e.HasIndex(b => b.Status);
            e.HasIndex(b => b.CustomerId);
            e.HasIndex(b => b.WorkerProfileId);
            e.HasQueryFilter(b => !b.IsDeleted);
        });

        builder.Entity<Review>(e =>
        {
            e.HasOne(r => r.Booking)
                .WithMany()
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.WorkerProfile)
                .WithMany()
                .HasForeignKey(r => r.WorkerProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(r => r.BookingId).IsUnique();
        });

        builder.Entity<Payment>(e =>
        {
            e.HasOne(p => p.Booking)
                .WithMany()
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(p => p.Amount).HasPrecision(18, 2);
            e.Property(p => p.CommissionAmount).HasPrecision(18, 2);
            e.HasIndex(p => p.BookingId).IsUnique();
        });

        builder.Entity<Invoice>(e =>
        {
            e.HasOne(i => i.Booking)
                .WithMany()
                .HasForeignKey(i => i.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(i => i.Amount).HasPrecision(18, 2);
            e.Property(i => i.CommissionAmount).HasPrecision(18, 2);
            e.Property(i => i.WorkerAmount).HasPrecision(18, 2);
            e.HasIndex(i => i.InvoiceNumber).IsUnique();
        });

        builder.Entity<Favorite>(e =>
        {
            e.HasIndex(f => new { f.CustomerId, f.WorkerProfileId, f.WorkerServiceId });
        });

        builder.Entity<AuditLog>(e =>
        {
            e.HasIndex(a => a.Timestamp);
            e.HasIndex(a => a.UserId);
        });

        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(rt => rt.Token);
            e.HasIndex(rt => rt.UserId);
        });
    }
}
