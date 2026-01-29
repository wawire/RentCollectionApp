using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Organizations;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class OrganizationService : IOrganizationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(ApplicationDbContext context, ILogger<OrganizationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<OrganizationDto>> CreateOrganizationAsync(CreateOrganizationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<OrganizationDto>.Failure("Organization name is required");
        }

        var exists = await _context.Organizations.AnyAsync(o => o.Name == dto.Name);
        if (exists)
        {
            return Result<OrganizationDto>.Failure("Organization name already exists");
        }

        var organization = new Organization
        {
            Name = dto.Name.Trim(),
            Status = OrganizationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Organization created: {OrganizationId} {OrganizationName}", organization.Id, organization.Name);

        return Result<OrganizationDto>.Success(new OrganizationDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Status = organization.Status,
            ActivatedAt = organization.ActivatedAt,
            CreatedAt = organization.CreatedAt
        });
    }

    public async Task<Result<IEnumerable<OrganizationDto>>> GetOrganizationsAsync()
    {
        var organizations = await _context.Organizations
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => new OrganizationDto
            {
                Id = o.Id,
                Name = o.Name,
                Status = o.Status,
                ActivatedAt = o.ActivatedAt,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        return Result<IEnumerable<OrganizationDto>>.Success(organizations);
    }

    public async Task<Result<OrganizationDto>> GetOrganizationByIdAsync(int id)
    {
        var organization = await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization == null)
        {
            return Result<OrganizationDto>.Failure($"Organization with ID {id} not found");
        }

        return Result<OrganizationDto>.Success(new OrganizationDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Status = organization.Status,
            ActivatedAt = organization.ActivatedAt,
            CreatedAt = organization.CreatedAt
        });
    }

    public async Task<Result<OrganizationDto>> UpdateOrganizationStatusAsync(int id, OrganizationStatus status)
    {
        var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == id);
        if (organization == null)
        {
            return Result<OrganizationDto>.Failure($"Organization with ID {id} not found");
        }

        organization.Status = status;
        organization.ActivatedAt = status == OrganizationStatus.Active ? DateTime.UtcNow : organization.ActivatedAt;

        await _context.SaveChangesAsync();

        return Result<OrganizationDto>.Success(new OrganizationDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Status = organization.Status,
            ActivatedAt = organization.ActivatedAt,
            CreatedAt = organization.CreatedAt
        }, "Organization status updated");
    }

    public async Task<Result> AssignUserToPropertyAsync(int organizationId, int propertyId, AssignUserToPropertyDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
        if (user == null)
        {
            return Result.Failure($"User with ID {dto.UserId} not found");
        }

        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId);
        if (property == null)
        {
            return Result.Failure($"Property with ID {propertyId} not found");
        }

        if (user.OrganizationId != organizationId || property.OrganizationId != organizationId)
        {
            return Result.Failure("User and property must belong to the same organization");
        }

        if (dto.AssignmentRole != UserRole.Manager &&
            dto.AssignmentRole != UserRole.Caretaker &&
            dto.AssignmentRole != UserRole.Accountant)
        {
            return Result.Failure("Assignment role must be Manager, Caretaker, or Accountant");
        }

        var existing = await _context.UserPropertyAssignments
            .FirstOrDefaultAsync(a => a.UserId == dto.UserId &&
                                      a.PropertyId == propertyId &&
                                      a.AssignmentRole == dto.AssignmentRole);

        if (existing == null)
        {
            var assignment = new UserPropertyAssignment
            {
                UserId = dto.UserId,
                PropertyId = propertyId,
                AssignmentRole = dto.AssignmentRole,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserPropertyAssignments.Add(assignment);
        }
        else
        {
            existing.IsActive = true;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.UserPropertyAssignments.Update(existing);
        }

        await _context.SaveChangesAsync();

        return Result.Success("User assigned to property successfully");
    }
}
