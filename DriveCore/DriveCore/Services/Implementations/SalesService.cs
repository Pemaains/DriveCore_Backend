using DriveCore.Data;
using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;
using DriveCore.Models;
using DriveCore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DriveCore.Services.Implementations
{
    public class SalesService : ISalesService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public SalesService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<ServiceResult<PartResponse>> CreatePartAsync(CreatePartRequest request)
        {
            var part = new Part
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                UnitPrice = request.UnitPrice,
                StockQuantity = request.StockQuantity
            };

            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            return ServiceResult<PartResponse>.Ok(MapPart(part), "Part created successfully.");
        }

        public async Task<List<PartResponse>> GetPartsAsync()
        {
            return await _context.Parts
                .OrderBy(part => part.Name)
                .Select(part => MapPart(part))
                .ToListAsync();
        }

        public async Task<ServiceResult<SalesInvoiceResponse>> CreateInvoiceAsync(CreateSalesInvoiceRequest request, string staffUserId)
        {
            var staff = await _context.StaffProfiles
                .FirstOrDefaultAsync(profile => profile.UserId == staffUserId);

            if (staff is null)
            {
                return ServiceResult<SalesInvoiceResponse>.Fail("Staff profile was not found.");
            }

            var customer = await _context.CustomerProfiles
                .Include(profile => profile.User)
                .FirstOrDefaultAsync(profile => profile.Id == request.CustomerProfileId);

            if (customer is null)
            {
                return ServiceResult<SalesInvoiceResponse>.Fail("Customer was not found.");
            }

            if (request.VehicleId.HasValue)
            {
                var vehicleExists = await _context.Vehicles.AnyAsync(vehicle => vehicle.Id == request.VehicleId
                    && vehicle.CustomerProfileId == request.CustomerProfileId);

                if (!vehicleExists)
                {
                    return ServiceResult<SalesInvoiceResponse>.Fail("Vehicle was not found for this customer.");
                }
            }

            var itemRequests = request.Items
                .Where(item => item.Quantity > 0)
                .ToList();

            if (itemRequests.Count == 0)
            {
                return ServiceResult<SalesInvoiceResponse>.Fail("At least one item is required.");
            }

            var partIds = itemRequests.Select(item => item.PartId).Distinct().ToList();
            var parts = await _context.Parts
                .Where(part => partIds.Contains(part.Id))
                .ToListAsync();

            if (parts.Count != partIds.Count)
            {
                return ServiceResult<SalesInvoiceResponse>.Fail("One or more parts were not found.");
            }

            var partsById = parts.ToDictionary(part => part.Id);
            var items = new List<SalesInvoiceItem>();

            foreach (var itemRequest in itemRequests)
            {
                var part = partsById[itemRequest.PartId];
                if (part.StockQuantity < itemRequest.Quantity)
                {
                    return ServiceResult<SalesInvoiceResponse>.Fail($"Insufficient stock for part: {part.Name}.");
                }

                var lineTotal = part.UnitPrice * itemRequest.Quantity;
                items.Add(new SalesInvoiceItem
                {
                    PartId = part.Id,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = part.UnitPrice,
                    LineTotal = lineTotal
                });

                part.StockQuantity -= itemRequest.Quantity;
            }

            var invoice = new SalesInvoice
            {
                CustomerProfileId = customer.Id,
                StaffProfileId = staff.Id,
                VehicleId = request.VehicleId,
                InvoiceNumber = GenerateInvoiceNumber(),
                TotalAmount = items.Sum(item => item.LineTotal),
                Items = items
            };

            _context.SalesInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            await _context.Entry(invoice)
                .Reference(saved => saved.CustomerProfile)
                .Query()
                .Include(profile => profile.User)
                .LoadAsync();

            await _context.Entry(invoice)
                .Collection(saved => saved.Items)
                .Query()
                .Include(item => item.Part)
                .LoadAsync();

            return ServiceResult<SalesInvoiceResponse>.Ok(MapInvoice(invoice), "Invoice created successfully.");
        }

        public async Task<ServiceResult<SalesInvoiceResponse>> GetInvoiceByIdAsync(int id)
        {
            var invoice = await _context.SalesInvoices
                .Include(sales => sales.CustomerProfile)
                    .ThenInclude(profile => profile.User)
                .Include(sales => sales.Items)
                    .ThenInclude(item => item.Part)
                .FirstOrDefaultAsync(sales => sales.Id == id);

            return invoice is null
                ? ServiceResult<SalesInvoiceResponse>.Fail("Invoice was not found.")
                : ServiceResult<SalesInvoiceResponse>.Ok(MapInvoice(invoice));
        }

        public async Task<ServiceResult<bool>> SendInvoiceAsync(int id)
            {

            var invoice = await _context.SalesInvoices
                .Include(sales => sales.CustomerProfile)
                    .ThenInclude(profile => profile.User)
                .Include(sales => sales.Items)
                    .ThenInclude(item => item.Part)
                .FirstOrDefaultAsync(sales => sales.Id == id);

            if (invoice is null)
            {
                return ServiceResult<bool>.Fail("Invoice was not found.");
            }

            var customerEmail = invoice.CustomerProfile.User.Email;
            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                return ServiceResult<bool>.Fail("Customer email is missing.");
            }

            try
            {
                var body = BuildInvoiceEmailBody(invoice);
                await _emailService.SendAsync(customerEmail, $"Invoice {invoice.InvoiceNumber}", body);

                return ServiceResult<bool>.Ok(true, "Invoice sent successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail("Failed to send invoice email.", new[] { ex.Message });
            }
        }

        private static string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        }

        private static PartResponse MapPart(Part part)
        {
            return new PartResponse
            {
                Id = part.Id,
                Name = part.Name,
                Description = part.Description,
                UnitPrice = part.UnitPrice,
                StockQuantity = part.StockQuantity
            };
        }

        private static SalesInvoiceResponse MapInvoice(SalesInvoice invoice)
        {
            return new SalesInvoiceResponse
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CreatedAt = invoice.CreatedAt,
                TotalAmount = invoice.TotalAmount,
                CustomerProfileId = invoice.CustomerProfileId,
                CustomerName = invoice.CustomerProfile.User.FullName,
                CustomerEmail = invoice.CustomerProfile.User.Email ?? string.Empty,
                VehicleId = invoice.VehicleId,
                Items = invoice.Items.Select(MapInvoiceItem).ToList()
            };
        }

        private static SalesInvoiceItemResponse MapInvoiceItem(SalesInvoiceItem item)
        {
            return new SalesInvoiceItemResponse
            {
                Id = item.Id,
                PartId = item.PartId,
                PartName = item.Part.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal
            };
        }

        private static string BuildInvoiceEmailBody(SalesInvoice invoice)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Hello {invoice.CustomerProfile.User.FullName},");
            builder.AppendLine();
            builder.AppendLine($"Invoice Number: {invoice.InvoiceNumber}");
            builder.AppendLine($"Date: {invoice.CreatedAt:yyyy-MM-dd}");
            builder.AppendLine();
            builder.AppendLine("Items:");

            foreach (var item in invoice.Items)
            {
                builder.AppendLine($"- {item.Part.Name} x {item.Quantity} @ {item.UnitPrice:C} = {item.LineTotal:C}");
            }

            builder.AppendLine();
            builder.AppendLine($"Total: {invoice.TotalAmount:C}");
            builder.AppendLine();
            builder.AppendLine("Thank you for your purchase.");

            return builder.ToString();
        }
    }
}
