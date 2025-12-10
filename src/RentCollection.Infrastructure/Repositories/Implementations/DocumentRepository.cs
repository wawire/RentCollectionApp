using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations
{
    public class DocumentRepository : Repository<Document>, IDocumentRepository
    {
        public DocumentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Document?> GetDocumentWithDetailsAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Tenant)
                .Include(d => d.UploadedBy)
                .Include(d => d.VerifiedBy)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<List<Document>> GetByTenantIdAsync(int tenantId)
        {
            return await _context.Documents
                .Include(d => d.Tenant)
                .Include(d => d.UploadedBy)
                .Include(d => d.VerifiedBy)
                .Where(d => d.TenantId == tenantId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<List<Document>> GetByPropertyIdAsync(int propertyId)
        {
            return await _context.Documents
                .Include(d => d.Tenant)
                .Include(d => d.UploadedBy)
                .Include(d => d.VerifiedBy)
                .Where(d => d.PropertyId == propertyId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<List<Document>> GetDocumentsByTenantIdAsync(int tenantId)
        {
            return await GetByTenantIdAsync(tenantId);
        }

        public async Task<List<Document>> GetDocumentsByPropertyIdAsync(int propertyId)
        {
            return await GetByPropertyIdAsync(propertyId);
        }

        public async Task<List<Document>> GetDocumentsByUnitIdAsync(int unitId)
        {
            return await _context.Documents
                .Include(d => d.Tenant)
                .Include(d => d.UploadedBy)
                .Include(d => d.VerifiedBy)
                .Where(d => d.Tenant!.UnitId == unitId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<List<Document>> GetDocumentsByTypeAsync(Domain.Enums.DocumentType documentType)
        {
            return await _context.Documents
                .Include(d => d.Tenant)
                .Include(d => d.UploadedBy)
                .Include(d => d.VerifiedBy)
                .Where(d => d.DocumentType == documentType)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<List<Document>> GetUnverifiedDocumentsAsync()
        {
            return await _context.Documents
                .Include(d => d.Tenant)
                .Include(d => d.UploadedBy)
                .Where(d => !d.IsVerified)
                .OrderBy(d => d.UploadedAt)
                .ToListAsync();
        }
    }
}
