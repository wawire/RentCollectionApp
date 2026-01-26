using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Expenses;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;

namespace RentCollection.API.Controllers;

/// <summary>
/// Expense tracking and management endpoints
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(
        IExpenseService expenseService,
        ICurrentUserService currentUserService,
        ILogger<ExpensesController> logger)
    {
        _expenseService = expenseService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    #region Query Operations

    /// <summary>
    /// Get all expenses for current landlord
    /// </summary>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of expenses</returns>
    [HttpGet]
    [Authorize(Roles = "PlatformAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllExpenses([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var landlordId = int.Parse(_currentUserService.UserId!);
            var expenses = await _expenseService.GetExpensesByLandlordAsync(landlordId, startDate, endDate);

            return Ok(new { success = true, data = expenses, count = expenses.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expenses");
            return BadRequest(new { success = false, message = "Failed to retrieve expenses" });
        }
    }

    /// <summary>
    /// Get expense by ID
    /// </summary>
    /// <param name="id">Expense ID</param>
    /// <returns>Expense details</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExpenseById(int id)
    {
        try
        {
            var expense = await _expenseService.GetExpenseByIdAsync(id);
            return Ok(new { success = true, data = expense });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Expense {ExpenseId} not found", id);
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expense {ExpenseId}", id);
            return BadRequest(new { success = false, message = "Failed to retrieve expense" });
        }
    }

    /// <summary>
    /// Get expenses for a specific property
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of expenses for the property</returns>
    [HttpGet("property/{propertyId}")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExpensesByProperty(int propertyId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var expenses = await _expenseService.GetExpensesByPropertyAsync(propertyId, startDate, endDate);
            return Ok(new { success = true, data = expenses, count = expenses.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expenses for property {PropertyId}", propertyId);
            return BadRequest(new { success = false, message = "Failed to retrieve property expenses" });
        }
    }

    /// <summary>
    /// Get expenses by category for current landlord
    /// </summary>
    /// <param name="category">Expense category</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of expenses in the category</returns>
    [HttpGet("category/{category}")]
    [Authorize(Roles = "PlatformAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExpensesByCategory(ExpenseCategory category, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var landlordId = int.Parse(_currentUserService.UserId!);
            var expenses = await _expenseService.GetExpensesByCategoryAsync(landlordId, category, startDate, endDate);

            return Ok(new { success = true, data = expenses, count = expenses.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expenses for category {Category}", category);
            return BadRequest(new { success = false, message = "Failed to retrieve category expenses" });
        }
    }

    /// <summary>
    /// Get recurring expenses for current landlord
    /// </summary>
    /// <returns>List of recurring expenses</returns>
    [HttpGet("recurring")]
    [Authorize(Roles = "PlatformAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRecurringExpenses()
    {
        try
        {
            var landlordId = int.Parse(_currentUserService.UserId!);
            var expenses = await _expenseService.GetRecurringExpensesAsync(landlordId);

            return Ok(new { success = true, data = expenses, count = expenses.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recurring expenses");
            return BadRequest(new { success = false, message = "Failed to retrieve recurring expenses" });
        }
    }

    #endregion

    #region Statistics & Analytics

    /// <summary>
    /// Get expense summary statistics for current landlord
    /// </summary>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>Expense summary with breakdowns and analytics</returns>
    [HttpGet("summary")]
    [Authorize(Roles = "PlatformAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExpenseSummary([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var landlordId = int.Parse(_currentUserService.UserId!);
            var summary = await _expenseService.GetExpenseSummaryAsync(landlordId, startDate, endDate);

            return Ok(new { success = true, data = summary });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expense summary");
            return BadRequest(new { success = false, message = "Failed to retrieve expense summary" });
        }
    }

    /// <summary>
    /// Get total expenses for current landlord
    /// </summary>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>Total expense amount</returns>
    [HttpGet("total")]
    [Authorize(Roles = "PlatformAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTotalExpenses([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var landlordId = int.Parse(_currentUserService.UserId!);
            var total = await _expenseService.GetTotalExpensesAsync(landlordId, startDate, endDate);

            return Ok(new { success = true, data = new { total } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total expenses");
            return BadRequest(new { success = false, message = "Failed to retrieve total expenses" });
        }
    }

    /// <summary>
    /// Get total expenses for a specific property
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>Total expense amount for the property</returns>
    [HttpGet("total/property/{propertyId}")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTotalExpensesByProperty(int propertyId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var total = await _expenseService.GetTotalExpensesByPropertyAsync(propertyId, startDate, endDate);
            return Ok(new { success = true, data = new { total } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total expenses for property {PropertyId}", propertyId);
            return BadRequest(new { success = false, message = "Failed to retrieve property total expenses" });
        }
    }

    #endregion

    #region CRUD Operations

    /// <summary>
    /// Create a new expense
    /// </summary>
    /// <param name="dto">Expense details</param>
    /// <returns>Created expense</returns>
    [HttpPost]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto dto)
    {
        try
        {
            var expense = await _expenseService.CreateExpenseAsync(dto);

            return CreatedAtAction(
                nameof(GetExpenseById),
                new { id = expense.Id },
                new { success = true, data = expense, message = "Expense created successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating expense");
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to create expense");
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            return BadRequest(new { success = false, message = "Failed to create expense" });
        }
    }

    /// <summary>
    /// Update an existing expense
    /// </summary>
    /// <param name="id">Expense ID</param>
    /// <param name="dto">Updated expense details</param>
    /// <returns>Updated expense</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "PlatformAdmin,Landlord,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateExpense(int id, [FromBody] UpdateExpenseDto dto)
    {
        try
        {
            var expense = await _expenseService.UpdateExpenseAsync(id, dto);
            return Ok(new { success = true, data = expense, message = "Expense updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Expense {ExpenseId} not found", id);
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense {ExpenseId}", id);
            return BadRequest(new { success = false, message = "Failed to update expense" });
        }
    }

    /// <summary>
    /// Delete an expense
    /// </summary>
    /// <param name="id">Expense ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "PlatformAdmin,Landlord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        try
        {
            await _expenseService.DeleteExpenseAsync(id);
            return Ok(new { success = true, message = "Expense deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Expense {ExpenseId} not found", id);
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense {ExpenseId}", id);
            return BadRequest(new { success = false, message = "Failed to delete expense" });
        }
    }

    #endregion

    #region Background Processing

    /// <summary>
    /// Process recurring expenses (manual trigger - PlatformAdmin only)
    /// Creates new expense entries for recurring expenses that are due
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("process-recurring")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessRecurringExpenses()
    {
        try
        {
            await _expenseService.ProcessRecurringExpensesAsync();
            return Ok(new { success = true, message = "Recurring expenses processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing recurring expenses");
            return BadRequest(new { success = false, message = "Failed to process recurring expenses" });
        }
    }

    #endregion
}

