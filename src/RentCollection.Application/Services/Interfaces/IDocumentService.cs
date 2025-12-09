using RentCollection.Application.Common;
using RentCollection.Application.DTOs.Documents;

namespace RentCollection.Application.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<ServiceResult<DocumentDto>> UploadDocumentAsync(UploadDocumentDto dto, int uploadedByUserId);
        Task<ServiceResult<List<DocumentDto>>> GetMyDocumentsAsync();
        Task<ServiceResult<List<DocumentDto>>> GetTenantDocumentsAsync(int tenantId);
        Task<ServiceResult<DocumentDto>> VerifyDocumentAsync(int documentId, bool isVerified);
        Task<ServiceResult<bool>> DeleteDocumentAsync(int documentId);
    }
}
