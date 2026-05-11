using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;

namespace DriveCore.Services.Interfaces
{
    public interface ISalesService
    {
        Task<ServiceResult<PartResponse>> CreatePartAsync(CreatePartRequest request);
        Task<List<PartResponse>> GetPartsAsync();
        Task<ServiceResult<SalesInvoiceResponse>> CreateInvoiceAsync(CreateSalesInvoiceRequest request, string staffUserId);
        Task<ServiceResult<SalesInvoiceResponse>> GetInvoiceByIdAsync(int id);
        Task<ServiceResult<bool>> SendInvoiceAsync(int id);
    }
}
