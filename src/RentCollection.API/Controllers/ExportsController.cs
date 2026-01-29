using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.Authorization;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "PlatformAdmin,Landlord,Manager,Accountant")]
[Authorize(Policy = Policies.RequireVerifiedUser)]
[Authorize(Policy = Policies.RequireActiveOrganization)]
public class ExportsController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportsController(IExportService exportService)
    {
        _exportService = exportService;
    }

    [HttpGet("payments")]
    public async Task<IActionResult> ExportPayments([FromQuery] int? propertyId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var csv = await _exportService.ExportPaymentsAsync(propertyId, startDate, endDate);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "payments.csv");
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> ExportInvoices([FromQuery] int? propertyId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var csv = await _exportService.ExportInvoicesAsync(propertyId, startDate, endDate);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "invoices.csv");
    }

    [HttpGet("expenses")]
    public async Task<IActionResult> ExportExpenses([FromQuery] int? propertyId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var csv = await _exportService.ExportExpensesAsync(propertyId, startDate, endDate);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "expenses.csv");
    }

    [HttpGet("arrears")]
    public async Task<IActionResult> ExportArrears([FromQuery] int? propertyId = null)
    {
        var csv = await _exportService.ExportArrearsAsync(propertyId);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "arrears.csv");
    }
}

