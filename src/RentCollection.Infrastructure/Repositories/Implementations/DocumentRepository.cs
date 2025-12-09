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
