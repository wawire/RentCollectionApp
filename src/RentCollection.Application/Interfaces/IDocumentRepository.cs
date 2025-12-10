using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Interfaces
{
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<Document?> GetDocumentWithDetailsAsync(int id);
        Task<List<Document>> GetByTenantIdAsync(int tenantId);
        Task<List<Document>> GetByPropertyIdAsync(int propertyId);
        Task<List<Document>> GetDocumentsByTenantIdAsync(int tenantId);
        Task<List<Document>> GetDocumentsByPropertyIdAsync(int propertyId);
        Task<List<Document>> GetDocumentsByUnitIdAsync(int unitId);
        Task<List<Document>> GetDocumentsByTypeAsync(DocumentType documentType);
        Task<List<Document>> GetUnverifiedDocumentsAsync();
    }
}
