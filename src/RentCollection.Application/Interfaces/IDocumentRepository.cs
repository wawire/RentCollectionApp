using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    /// <summary>
    /// Get all documents for a specific tenant
    /// </summary>
    Task<IEnumerable<Document>> GetDocumentsByTenantIdAsync(int tenantId);

    /// <summary>
    /// Get all documents for a specific property
    /// </summary>
    Task<IEnumerable<Document>> GetDocumentsByPropertyIdAsync(int propertyId);

    /// <summary>
    /// Get all documents for a specific unit
    /// </summary>
    Task<IEnumerable<Document>> GetDocumentsByUnitIdAsync(int unitId);

    /// <summary>
    /// Get all documents of a specific type
    /// </summary>
    Task<IEnumerable<Document>> GetDocumentsByTypeAsync(DocumentType documentType);

    /// <summary>
    /// Get document with all related details
    /// </summary>
    Task<Document?> GetDocumentWithDetailsAsync(int id);

    /// <summary>
    /// Get all unverified documents
    /// </summary>
    Task<IEnumerable<Document>> GetUnverifiedDocumentsAsync();

    /// <summary>
    /// Get all documents uploaded by a specific user
    /// </summary>
    Task<IEnumerable<Document>> GetDocumentsByUploaderIdAsync(int uploaderId);
}
