using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Organizations;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services.Interfaces;

public interface IOrganizationService
{
    Task<Result<OrganizationDto>> CreateOrganizationAsync(CreateOrganizationDto dto);
    Task<Result<IEnumerable<OrganizationDto>>> GetOrganizationsAsync();
    Task<Result<OrganizationDto>> GetOrganizationByIdAsync(int id);
    Task<Result<OrganizationDto>> UpdateOrganizationStatusAsync(int id, OrganizationStatus status);
    Task<Result> AssignUserToPropertyAsync(int organizationId, int propertyId, AssignUserToPropertyDto dto);
}
