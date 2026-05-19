using DriveCore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DriveCore.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<StaffProfile> StaffProfiles => Set<StaffProfile>();
        public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<PartRequest> PartRequests => Set<PartRequest>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<LowStockNotification> LowStockNotifications { get; set; }
        public DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; }
        public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
        public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
        
        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<Part> Parts => Set<Part>();
        public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
        public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .Property(user => user.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Entity<StaffProfile>()
                .HasIndex(staff => staff.StaffCode)
                .IsUnique();

            builder.Entity<StaffProfile>()
                .HasOne(staff => staff.User)
                .WithOne(user => user.StaffProfile)
                .HasForeignKey<StaffProfile>(staff => staff.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CustomerProfile>()
                .HasOne(customer => customer.User)
                .WithOne(user => user.CustomerProfile)
                .HasForeignKey<CustomerProfile>(customer => customer.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CustomerProfile>()
                .HasOne(customer => customer.CreatedByStaff)
                .WithMany()
                .HasForeignKey(customer => customer.CreatedByStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Vehicle>()
                .HasIndex(vehicle => vehicle.VehicleNumber)
                .IsUnique();

            builder.Entity<Vehicle>()
                .HasOne(vehicle => vehicle.CustomerProfile)
                .WithMany(customer => customer.Vehicles)
                .HasForeignKey(vehicle => vehicle.CustomerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SalesInvoice>()
            .HasIndex(invoice => invoice.InvoiceNumber)
            .IsUnique();

        builder.Entity<SalesInvoice>()
            .HasOne(invoice => invoice.CustomerProfile)
            .WithMany(customer => customer.SalesInvoices)
            .HasForeignKey(invoice => invoice.CustomerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SalesInvoice>()
            .HasOne(invoice => invoice.StaffProfile)
            .WithMany(staff => staff.SalesInvoices)
            .HasForeignKey(invoice => invoice.StaffProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SalesInvoice>()
            .HasOne(invoice => invoice.Vehicle)
            .WithMany(vehicle => vehicle.SalesInvoices)
            .HasForeignKey(invoice => invoice.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<SalesInvoiceItem>()
            .HasOne(item => item.SalesInvoice)
            .WithMany(invoice => invoice.Items)
            .HasForeignKey(item => item.SalesInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SalesInvoiceItem>()
            .HasOne(item => item.Part)
            .WithMany(part => part.SalesInvoiceItems)
            .HasForeignKey(item => item.PartId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Part>()
            .HasIndex(p => p.PartNumber)
            .IsUnique();

        builder.Entity<PurchaseInvoice>()
            .HasIndex(p => p.InvoiceNumber)
            .IsUnique();

        builder.Entity<Part>()
            .HasOne(p => p.PreferredVendor)
            .WithMany()
            .HasForeignKey(p => p.PreferredVendorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PurchaseInvoiceItem>()
            .HasOne(i => i.PurchaseInvoice)
            .WithMany(i => i.Items)
            .HasForeignKey(i => i.PurchaseInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
