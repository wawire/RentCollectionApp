using Microsoft.AspNetCore.Http;
using RentCollection.Application.Common;
using RentCollection.Application.DTOs.BulkImport;

namespace RentCollection.Application.Services.Interfaces
{
    public interface IBulkImportService
    {
        Task<ServiceResult<BulkImportResultDto>> ImportTenantsFromCsvAsync(IFormFile file, int propertyId);
        Task<ServiceResult<BulkImportResultDto>> ImportPaymentsFromCsvAsync(IFormFile file);
        Task<BulkImportResultDto> ValidateTenantCsvAsync(IFormFile file, int propertyId);
        Task<BulkImportResultDto> ValidatePaymentCsvAsync(IFormFile file);
    }
}
