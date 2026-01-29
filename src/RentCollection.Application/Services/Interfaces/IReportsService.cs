using RentCollection.Application.Common;
using RentCollection.Application.DTOs.Reports;

namespace RentCollection.Application.Services.Interfaces
{
    public interface IReportsService
    {
        Task<ServiceResult<ProfitLossReportDto>> GenerateProfitLossReportAsync(DateTime startDate, DateTime endDate, int? landlordId = null, IReadOnlyCollection<int>? propertyIds = null, int? organizationId = null);
        Task<ServiceResult<ArrearsReportDto>> GenerateArrearsReportAsync(int? landlordId = null, IReadOnlyCollection<int>? propertyIds = null, int? organizationId = null);
        Task<ServiceResult<OccupancyReportDto>> GenerateOccupancyReportAsync(int? landlordId = null, IReadOnlyCollection<int>? propertyIds = null, int? organizationId = null);
}
}
