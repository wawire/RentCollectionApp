using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations;

public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Document>> GetDocumentsByTenantIdAsync(int tenantId)
    {
        return await _context.Documents
            .Include(d => d.Tenant)
                .ThenInclude(t => t!.Unit)
                    .ThenInclude(u => u.Property)
            .Include(d => d.UploadedBy)
            .Include(d => d.VerifiedBy)
            .Where(d => d.TenantId == tenantId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetDocumentsByPropertyIdAsync(int propertyId)
    {
        return await _context.Documents
            .Include(d => d.Property)
            .Include(d => d.Tenant)
                .ThenInclude(t => t!.Unit)
            .Include(d => d.UploadedBy)
            .Include(d => d.VerifiedBy)
            .Where(d => d.PropertyId == propertyId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetDocumentsByUnitIdAsync(int unitId)
    {
        return await _context.Documents
            .Include(d => d.Unit)
                .ThenInclude(u => u!.Property)
            .Include(d => d.Tenant)
            .Include(d => d.UploadedBy)
            .Include(d => d.VerifiedBy)
            .Where(d => d.UnitId == unitId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetDocumentsByTypeAsync(DocumentType documentType)
    {
        return await _context.Documents
            .Include(d => d.Tenant)
                .ThenInclude(t => t!.Unit)
                    .ThenInclude(u => u.Property)
            .Include(d => d.Property)
            .Include(d => d.Unit)
                .ThenInclude(u => u!.Property)
            .Include(d => d.UploadedBy)
            .Include(d => d.VerifiedBy)
            .Where(d => d.DocumentType == documentType)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<Document?> GetDocumentWithDetailsAsync(int id)
    {
        return await _context.Documents
            .Include(d => d.Tenant)
                .ThenInclude(t => t!.Unit)
                    .ThenInclude(u => u.Property)
            .Include(d => d.Property)
            .Include(d => d.Unit)
                .ThenInclude(u => u!.Property)
            .Include(d => d.UploadedBy)
            .Include(d => d.VerifiedBy)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Document>> GetUnverifiedDocumentsAsync()
    {
        return await _context.Documents
            .Include(d => d.Tenant)
                .ThenInclude(t => t!.Unit)
                    .ThenInclude(u => u.Property)
            .Include(d => d.Property)
            .Include(d => d.Unit)
                .ThenInclude(u => u!.Property)
            .Include(d => d.UploadedBy)
            .Where(d => !d.IsVerified)
            .OrderBy(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetDocumentsByUploaderIdAsync(int uploaderId)
    {
        return await _context.Documents
            .Include(d => d.Tenant)
                .ThenInclude(t => t!.Unit)
                    .ThenInclude(u => u.Property)
            .Include(d => d.Property)
            .Include(d => d.Unit)
                .ThenInclude(u => u!.Property)
            .Include(d => d.UploadedBy)
            .Include(d => d.VerifiedBy)
            .Where(d => d.UploadedByUserId == uploaderId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Document>> GetAllAsync()
    {
        return await _context.Documents
            .Include(d => d.Tenant)
                .ThenInclude(t => t!.Unit)
                    .ThenInclude(u => u.Property)
            .Include(d => d.Property)
            .Include(d => d.Unit)
                .ThenInclude(u => u!.Property)
            .Include(d => d.UploadedBy)
            .Include(d => d.VerifiedBy)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }
}
