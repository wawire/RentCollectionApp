using RentCollection.Application.Common;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Documents;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<ServiceResult<DocumentDto>> UploadDocumentAsync(UploadDocumentDto dto, int uploadedByUserId);
        Task<ServiceResult<List<DocumentDto>>> GetMyDocumentsAsync();
        Task<ServiceResult<List<DocumentDto>>> GetTenantDocumentsAsync(int tenantId);
        Task<Result<IEnumerable<DocumentDto>>> GetAllDocumentsAsync();
        Task<Result<DocumentDto>> GetDocumentByIdAsync(int id);
        Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByTenantIdAsync(int tenantId);
        Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByPropertyIdAsync(int propertyId);
        Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByUnitIdAsync(int unitId);
        Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByTypeAsync(DocumentType documentType);
        Task<Result<IEnumerable<DocumentDto>>> GetUnverifiedDocumentsAsync();
        Task<ServiceResult<DocumentDto>> VerifyDocumentAsync(int documentId, bool isVerified);
        Task<ServiceResult<bool>> DeleteDocumentAsync(int documentId);
    }
}
