using Microsoft.AspNetCore.Http;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Documents;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services.Interfaces;

public interface IDocumentService
{
    /// <summary>
    /// Upload a new document
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="uploadDto">Document metadata</param>
    /// <returns>Uploaded document information</returns>
    Task<Result<DocumentDto>> UploadDocumentAsync(IFormFile file, UploadDocumentDto uploadDto);

    /// <summary>
    /// Get all documents (with RBAC filtering)
    /// </summary>
    /// <returns>List of documents</returns>
    Task<Result<IEnumerable<DocumentDto>>> GetAllDocumentsAsync();

    /// <summary>
    /// Get document by ID (with RBAC verification)
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Document information</returns>
    Task<Result<DocumentDto>> GetDocumentByIdAsync(int id);

    /// <summary>
    /// Get all documents for a specific tenant (with RBAC verification)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>List of documents</returns>
    Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByTenantIdAsync(int tenantId);

    /// <summary>
    /// Get all documents for a specific property (with RBAC verification)
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <returns>List of documents</returns>
    Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByPropertyIdAsync(int propertyId);

    /// <summary>
    /// Get all documents for a specific unit (with RBAC verification)
    /// </summary>
    /// <param name="unitId">Unit ID</param>
    /// <returns>List of documents</returns>
    Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByUnitIdAsync(int unitId);

    /// <summary>
    /// Get all documents of a specific type (with RBAC filtering)
    /// </summary>
    /// <param name="documentType">Document type</param>
    /// <returns>List of documents</returns>
    Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByTypeAsync(DocumentType documentType);

    /// <summary>
    /// Get all unverified documents (landlord/admin only)
    /// </summary>
    /// <returns>List of unverified documents</returns>
    Task<Result<IEnumerable<DocumentDto>>> GetUnverifiedDocumentsAsync();

    /// <summary>
    /// Get my documents (for current tenant user)
    /// </summary>
    /// <returns>List of documents</returns>
    Task<Result<IEnumerable<DocumentDto>>> GetMyDocumentsAsync();

    /// <summary>
    /// Verify/approve a document (landlord/admin only)
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="verifyDto">Verification data</param>
    /// <returns>Updated document</returns>
    Task<Result<DocumentDto>> VerifyDocumentAsync(int id, VerifyDocumentDto verifyDto);

    /// <summary>
    /// Delete a document (with RBAC verification)
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Success result</returns>
    Task<Result> DeleteDocumentAsync(int id);
}
