using DriveCore.Data;
using DriveCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DriveCore.Services
{
    public static class SeedData
    {
        public static async Task SeedAsync(AppDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            var staffEmail = configuration["SampleData:StaffEmail"];
            var staffPassword = configuration["SampleData:StaffPassword"];
            var customerEmail = configuration["SampleData:CustomerEmail"];
            var customerPassword = configuration["SampleData:CustomerPassword"];

            if (string.IsNullOrWhiteSpace(staffEmail) || string.IsNullOrWhiteSpace(staffPassword)
                || string.IsNullOrWhiteSpace(customerEmail) || string.IsNullOrWhiteSpace(customerPassword))
            {
                return;
            }

            var staffUser = await userManager.FindByEmailAsync(staffEmail);
            if (staffUser is null)
            {
                staffUser = new ApplicationUser
                {
                    FullName = "Sample Staff",
                    Email = staffEmail,
                    UserName = staffEmail,
                    PhoneNumber = "9800000001",
                    Role = UserRole.Staff,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var staffResult = await userManager.CreateAsync(staffUser, staffPassword);
                if (!staffResult.Succeeded)
                {
                    return;
                }

                await userManager.AddToRoleAsync(staffUser, UserRole.Staff.ToString());

                context.StaffProfiles.Add(new StaffProfile
                {
                    UserId = staffUser.Id,
                    StaffCode = "STF-001",
                    Position = "Sales"
                });
                await context.SaveChangesAsync();
            }

            var customerUser = await userManager.FindByEmailAsync(customerEmail);
            if (customerUser is null)
            {
                customerUser = new ApplicationUser
                {
                    FullName = "Sample Customer",
                    Email = customerEmail,
                    UserName = customerEmail,
                    PhoneNumber = "9800000002",
                    Role = UserRole.Customer,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var customerResult = await userManager.CreateAsync(customerUser, customerPassword);
                if (!customerResult.Succeeded)
                {
                    return;
                }

                await userManager.AddToRoleAsync(customerUser, UserRole.Customer.ToString());

                context.CustomerProfiles.Add(new CustomerProfile
                {
                    UserId = customerUser.Id,
                    Address = "Sample Address",
                    Vehicles = new List<Vehicle>
                    {
                        new Vehicle
                        {
                            VehicleNumber = "BA-1-PA-1234",
                            Brand = "Toyota",
                            Model = "Corolla",
                            Year = 2022,
                            Color = "White"
                        }
                    }
                });
                await context.SaveChangesAsync();
            }

            if (!await context.Parts.AnyAsync())
            {
                context.Parts.AddRange(
                    new Part
                    {
                        Name = "Oil Filter",
                        Description = "Standard oil filter",
                        UnitPrice = 15.00m,
                        StockQuantity = 50
                    },
                    new Part
                    {
                        Name = "Brake Pad",
                        Description = "Front brake pad",
                        UnitPrice = 45.00m,
                        StockQuantity = 30
                    });

                await context.SaveChangesAsync();
            }

            var staffProfile = await context.StaffProfiles.FirstOrDefaultAsync(profile => profile.UserId == staffUser.Id);
            var customerProfile = await context.CustomerProfiles
                .Include(profile => profile.Vehicles)
                .FirstOrDefaultAsync(profile => profile.UserId == customerUser.Id);

            if (staffProfile is null || customerProfile is null)
            {
                return;
            }

            if (!await context.SalesInvoices.AnyAsync())
            {
                var part = await context.Parts.FirstAsync();
                var item = new SalesInvoiceItem
                {
                    PartId = part.Id,
                    Quantity = 2,
                    UnitPrice = part.UnitPrice,
                    LineTotal = part.UnitPrice * 2
                };

                var invoice = new SalesInvoice
                {
                    CustomerProfileId = customerProfile.Id,
                    StaffProfileId = staffProfile.Id,
                    VehicleId = customerProfile.Vehicles.FirstOrDefault()?.Id,
                    InvoiceNumber = $"INV-SEED-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    TotalAmount = item.LineTotal,
                    Items = new List<SalesInvoiceItem> { item }
                };

                context.SalesInvoices.Add(invoice);
                await context.SaveChangesAsync();
            }
        }
    }
}
