using RentCollection.Domain.Entities;

namespace RentCollection.Application.Interfaces
{
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<List<Document>> GetByTenantIdAsync(int tenantId);
        Task<List<Document>> GetByPropertyIdAsync(int propertyId);
        Task<List<Document>> GetUnverifiedDocumentsAsync();
    }
}
