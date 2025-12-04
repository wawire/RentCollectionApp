using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Application.Services.Implementations;

public class LandlordPaymentAccountService : ILandlordPaymentAccountService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<LandlordPaymentAccountService> _logger;

    public LandlordPaymentAccountService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<LandlordPaymentAccountService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<List<LandlordPaymentAccountDto>>> GetLandlordAccountsAsync(int landlordId)
    {
        try
        {
            var accounts = await _context.LandlordPaymentAccounts
                .Include(a => a.Property)
                .Where(a => a.LandlordId == landlordId)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.AccountName)
                .ToListAsync();

            var accountDtos = _mapper.Map<List<LandlordPaymentAccountDto>>(accounts);

            return ServiceResult<List<LandlordPaymentAccountDto>>.Success(accountDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment accounts for landlord {LandlordId}", landlordId);
            return ServiceResult<List<LandlordPaymentAccountDto>>.Failure("An error occurred while retrieving payment accounts");
        }
    }

    public async Task<ServiceResult<List<LandlordPaymentAccountDto>>> GetPropertyAccountsAsync(int propertyId)
    {
        try
        {
            var accounts = await _context.LandlordPaymentAccounts
                .Include(a => a.Property)
                .Where(a => a.PropertyId == propertyId)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.AccountName)
                .ToListAsync();

            var accountDtos = _mapper.Map<List<LandlordPaymentAccountDto>>(accounts);

            return ServiceResult<List<LandlordPaymentAccountDto>>.Success(accountDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment accounts for property {PropertyId}", propertyId);
            return ServiceResult<List<LandlordPaymentAccountDto>>.Failure("An error occurred while retrieving payment accounts");
        }
    }

    public async Task<ServiceResult<LandlordPaymentAccountDto>> GetAccountByIdAsync(int accountId)
    {
        try
        {
            var account = await _context.LandlordPaymentAccounts
                .Include(a => a.Property)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
            {
                return ServiceResult<LandlordPaymentAccountDto>.Failure($"Payment account with ID {accountId} not found");
            }

            var accountDto = _mapper.Map<LandlordPaymentAccountDto>(account);

            return ServiceResult<LandlordPaymentAccountDto>.Success(accountDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment account {AccountId}", accountId);
            return ServiceResult<LandlordPaymentAccountDto>.Failure("An error occurred while retrieving the payment account");
        }
    }

    public async Task<ServiceResult<LandlordPaymentAccountDto>> GetDefaultAccountAsync(int landlordId, int? propertyId = null)
    {
        try
        {
            var query = _context.LandlordPaymentAccounts
                .Include(a => a.Property)
                .Where(a => a.LandlordId == landlordId && a.IsActive && a.IsDefault);

            if (propertyId.HasValue)
            {
                query = query.Where(a => a.PropertyId == propertyId.Value);
            }

            var account = await query.FirstOrDefaultAsync();

            if (account == null)
            {
                // Fallback: get any active account
                account = await _context.LandlordPaymentAccounts
                    .Include(a => a.Property)
                    .Where(a => a.LandlordId == landlordId && a.IsActive)
                    .FirstOrDefaultAsync();
            }

            if (account == null)
            {
                return ServiceResult<LandlordPaymentAccountDto>.Failure("No payment account found for this landlord");
            }

            var accountDto = _mapper.Map<LandlordPaymentAccountDto>(account);

            return ServiceResult<LandlordPaymentAccountDto>.Success(accountDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving default payment account for landlord {LandlordId}", landlordId);
            return ServiceResult<LandlordPaymentAccountDto>.Failure("An error occurred while retrieving the default payment account");
        }
    }

    public async Task<ServiceResult<LandlordPaymentAccountDto>> CreateAccountAsync(int landlordId, CreateLandlordPaymentAccountDto dto)
    {
        try
        {
            // If this is set as default, unset other defaults
            if (dto.IsDefault)
            {
                await UnsetDefaultAccountsAsync(landlordId, dto.PropertyId);
            }

            var account = _mapper.Map<LandlordPaymentAccount>(dto);
            account.LandlordId = landlordId;
            account.CreatedAt = DateTime.UtcNow;

            await _context.LandlordPaymentAccounts.AddAsync(account);
            await _context.SaveChangesAsync();

            // Reload with details
            var createdAccount = await _context.LandlordPaymentAccounts
                .Include(a => a.Property)
                .FirstOrDefaultAsync(a => a.Id == account.Id);

            var accountDto = _mapper.Map<LandlordPaymentAccountDto>(createdAccount);

            _logger.LogInformation("Payment account created: {AccountId} for landlord {LandlordId}", account.Id, landlordId);

            return ServiceResult<LandlordPaymentAccountDto>.Success(accountDto, "Payment account created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment account for landlord {LandlordId}", landlordId);
            return ServiceResult<LandlordPaymentAccountDto>.Failure("An error occurred while creating the payment account");
        }
    }

    public async Task<ServiceResult<LandlordPaymentAccountDto>> UpdateAccountAsync(int accountId, UpdateLandlordPaymentAccountDto dto)
    {
        try
        {
            var account = await _context.LandlordPaymentAccounts
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
            {
                return ServiceResult<LandlordPaymentAccountDto>.Failure($"Payment account with ID {accountId} not found");
            }

            // If this is set as default, unset other defaults
            if (dto.IsDefault && !account.IsDefault)
            {
                await UnsetDefaultAccountsAsync(account.LandlordId, account.PropertyId);
            }

            // Update account
            _mapper.Map(dto, account);
            account.UpdatedAt = DateTime.UtcNow;

            _context.LandlordPaymentAccounts.Update(account);
            await _context.SaveChangesAsync();

            // Reload with details
            var updatedAccount = await _context.LandlordPaymentAccounts
                .Include(a => a.Property)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            var accountDto = _mapper.Map<LandlordPaymentAccountDto>(updatedAccount);

            _logger.LogInformation("Payment account updated: {AccountId}", accountId);

            return ServiceResult<LandlordPaymentAccountDto>.Success(accountDto, "Payment account updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment account {AccountId}", accountId);
            return ServiceResult<LandlordPaymentAccountDto>.Failure("An error occurred while updating the payment account");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAccountAsync(int accountId)
    {
        try
        {
            var account = await _context.LandlordPaymentAccounts
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
            {
                return ServiceResult<bool>.Failure($"Payment account with ID {accountId} not found");
            }

            // Check if account is being used by any payments
            var hasPayments = await _context.Payments
                .AnyAsync(p => p.LandlordAccountId == accountId);

            if (hasPayments)
            {
                return ServiceResult<bool>.Failure("Cannot delete payment account that has associated payments");
            }

            _context.LandlordPaymentAccounts.Remove(account);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment account deleted: {AccountId}", accountId);

            return ServiceResult<bool>.Success(true, "Payment account deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment account {AccountId}", accountId);
            return ServiceResult<bool>.Failure("An error occurred while deleting the payment account");
        }
    }

    public async Task<ServiceResult<bool>> SetDefaultAccountAsync(int accountId)
    {
        try
        {
            var account = await _context.LandlordPaymentAccounts
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
            {
                return ServiceResult<bool>.Failure($"Payment account with ID {accountId} not found");
            }

            // Unset other defaults
            await UnsetDefaultAccountsAsync(account.LandlordId, account.PropertyId);

            // Set this as default
            account.IsDefault = true;
            account.UpdatedAt = DateTime.UtcNow;

            _context.LandlordPaymentAccounts.Update(account);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment account {AccountId} set as default", accountId);

            return ServiceResult<bool>.Success(true, "Account set as default successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting payment account {AccountId} as default", accountId);
            return ServiceResult<bool>.Failure("An error occurred while setting the account as default");
        }
    }

    private async Task UnsetDefaultAccountsAsync(int landlordId, int? propertyId)
    {
        var defaultAccounts = await _context.LandlordPaymentAccounts
            .Where(a => a.LandlordId == landlordId && a.IsDefault)
            .Where(a => a.PropertyId == propertyId || (!propertyId.HasValue && !a.PropertyId.HasValue))
            .ToListAsync();

        foreach (var account in defaultAccounts)
        {
            account.IsDefault = false;
            account.UpdatedAt = DateTime.UtcNow;
        }

        if (defaultAccounts.Any())
        {
            _context.LandlordPaymentAccounts.UpdateRange(defaultAccounts);
        }
    }
}
