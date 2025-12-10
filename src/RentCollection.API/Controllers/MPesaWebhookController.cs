using Microsoft.AspNetCore.Mvc;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.API.Controllers;

/// <summary>
/// M-Pesa webhook endpoints for receiving payment callbacks from Safaricom
/// These endpoints should be publicly accessible (no authentication)
/// </summary>
[ApiController]
[Route("api/mpesa")]
[Produces("application/json")]
public class MPesaWebhookController : ControllerBase
{
    private readonly IMPesaService _mpesaService;
    private readonly IMPesaTransactionService _mpesaTransactionService;
    private readonly ILogger<MPesaWebhookController> _logger;

    public MPesaWebhookController(
        IMPesaService mpesaService,
        IMPesaTransactionService mpesaTransactionService,
        ILogger<MPesaWebhookController> logger)
    {
        _mpesaService = mpesaService;
        _mpesaTransactionService = mpesaTransactionService;
        _logger = logger;
    }

    /// <summary>
    /// M-Pesa C2B validation endpoint
    /// Called by Safaricom to validate the payment before processing
    /// </summary>
    /// <param name="validationData">Validation data from M-Pesa</param>
    /// <returns>Validation response</returns>
    [HttpPost("c2b/validation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult C2BValidation([FromBody] object validationData)
    {
        _logger.LogInformation("M-Pesa C2B validation request received: {ValidationData}",
            System.Text.Json.JsonSerializer.Serialize(validationData));

        // For now, accept all payments
        // In production, you might want to validate:
        // - Account number exists
        // - Amount is reasonable
        // - Other business rules

        return Ok(new MPesaC2BCallbackResponseDto
        {
            ResultCode = 0,
            ResultDesc = "Accepted"
        });
    }

    /// <summary>
    /// M-Pesa C2B confirmation endpoint
    /// Called by Safaricom after a successful payment
    /// This is where we create the payment record
    /// </summary>
    /// <param name="confirmationData">Confirmation data from M-Pesa</param>
    /// <returns>Confirmation response</returns>
    [HttpPost("c2b/confirmation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> C2BConfirmation([FromBody] MPesaC2BCallbackDto confirmationData)
    {
        _logger.LogInformation("M-Pesa C2B confirmation request received: TransID={TransID}, Amount={Amount}, BillRef={BillRef}",
            confirmationData.TransID, confirmationData.TransAmount, confirmationData.BillRefNumber);

        var result = await _mpesaService.HandleC2BCallbackAsync(confirmationData);

        if (!result.IsSuccess)
        {
            _logger.LogError("Failed to process M-Pesa C2B callback: {Error}", result.ErrorMessage);

            // Still return success to M-Pesa to acknowledge receipt
            // We log the error for manual investigation
            return Ok(new MPesaC2BCallbackResponseDto
            {
                ResultCode = 0,
                ResultDesc = "Accepted" // Always acknowledge to M-Pesa
            });
        }

        return Ok(new MPesaC2BCallbackResponseDto
        {
            ResultCode = 0,
            ResultDesc = "Accepted"
        });
    }

    /// <summary>
    /// M-Pesa STK Push callback endpoint
    /// Called by Safaricom after customer completes or cancels STK Push
    /// </summary>
    /// <param name="callbackData">STK Push callback data</param>
    /// <returns>Callback response</returns>
    [HttpPost("stkpush/callback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> StkPushCallback([FromBody] StkPushCallbackRequestDto callbackData)
    {
        _logger.LogInformation("M-Pesa STK Push callback received: CheckoutRequestID={CheckoutRequestID}, ResultCode={ResultCode}",
            callbackData.Body?.StkCallback?.CheckoutRequestID,
            callbackData.Body?.StkCallback?.ResultCode);

        var result = await _mpesaTransactionService.HandleStkPushCallbackAsync(callbackData);

        if (!result.IsSuccess)
        {
            _logger.LogError("Failed to process STK Push callback: {Error}", result.ErrorMessage);
        }

        // Always return success to M-Pesa to acknowledge receipt
        return Ok(new
        {
            ResultCode = 0,
            ResultDesc = "Accepted"
        });
    }

    /// <summary>
    /// Health check endpoint for M-Pesa webhook
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "M-Pesa Webhook"
        });
    }
}
