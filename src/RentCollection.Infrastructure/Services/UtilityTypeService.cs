using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Utilities;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class UtilityTypeService : IUtilityTypeService
{
    private readonly ApplicationDbContext _context;

    public UtilityTypeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<UtilityTypeDto>>> GetUtilityTypesAsync(bool includeInactive = false)
    {
        var query = _context.UtilityTypes.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        var types = await query.OrderBy(t => t.Name).ToListAsync();
        var dtos = types.Select(t => new UtilityTypeDto
        {
            Id = t.Id,
            Name = t.Name,
            BillingMode = t.BillingMode,
            UnitOfMeasure = t.UnitOfMeasure,
            IsActive = t.IsActive,
            Description = t.Description
        }).ToList();

        return Result<List<UtilityTypeDto>>.Success(dtos);
    }

    public async Task<Result<UtilityTypeDto>> CreateUtilityTypeAsync(CreateUtilityTypeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<UtilityTypeDto>.Failure("Utility name is required");
        }

        var exists = await _context.UtilityTypes.AnyAsync(t => t.Name == dto.Name);
        if (exists)
        {
            return Result<UtilityTypeDto>.Failure("Utility type already exists");
        }

        var type = new UtilityType
        {
            Name = dto.Name.Trim(),
            BillingMode = dto.BillingMode,
            UnitOfMeasure = dto.UnitOfMeasure,
            Description = dto.Description
        };

        _context.UtilityTypes.Add(type);
        await _context.SaveChangesAsync();

        return Result<UtilityTypeDto>.Success(new UtilityTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            BillingMode = type.BillingMode,
            UnitOfMeasure = type.UnitOfMeasure,
            IsActive = type.IsActive,
            Description = type.Description
        });
    }
}
