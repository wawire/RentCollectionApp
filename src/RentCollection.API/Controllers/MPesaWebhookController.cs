using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Infrastructure.Configuration;

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
    private readonly MPesaConfiguration _mpesaConfiguration;
    private readonly IWebHostEnvironment _environment;

    public MPesaWebhookController(
        IMPesaService mpesaService,
        IMPesaTransactionService mpesaTransactionService,
        ILogger<MPesaWebhookController> logger,
        IOptions<MPesaConfiguration> mpesaConfiguration,
        IWebHostEnvironment environment)
    {
        _mpesaService = mpesaService;
        _mpesaTransactionService = mpesaTransactionService;
        _logger = logger;
        _mpesaConfiguration = mpesaConfiguration.Value;
        _environment = environment;
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
        var validationResult = ValidateWebhookRequest();
        if (validationResult != null)
        {
            return validationResult;
        }

        var correlationId = GetCorrelationId();
        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });

        _logger.LogInformation("M-Pesa C2B validation request received: {ValidationData}",
            JsonSerializer.Serialize(validationData));

        var validation = JsonSerializer.Deserialize<MPesaC2BCallbackDto>(
            JsonSerializer.Serialize(validationData),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (validation == null ||
            string.IsNullOrWhiteSpace(validation.BillRefNumber) ||
            validation.TransAmount <= 0 ||
            !Regex.IsMatch(validation.BillRefNumber, "^[A-Za-z0-9-]+$"))
        {
            return Ok(new MPesaC2BCallbackResponseDto
            {
                ResultCode = 1,
                ResultDesc = "Rejected"
            });
        }

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
        var validationResult = ValidateWebhookRequest();
        if (validationResult != null)
        {
            return validationResult;
        }

        var correlationId = GetCorrelationId();
        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });

        _logger.LogInformation("M-Pesa C2B confirmation request received: TransID={TransID}, Amount={Amount}, BillRef={BillRef}",
            confirmationData.TransID, confirmationData.TransAmount, confirmationData.BillRefNumber);

        var result = await _mpesaService.HandleC2BCallbackAsync(confirmationData, correlationId);

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
        var validationResult = ValidateWebhookRequest();
        if (validationResult != null)
        {
            return validationResult;
        }

        var correlationId = GetCorrelationId();
        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });

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
    /// M-Pesa B2C result callback endpoint
    /// Called by Safaricom after B2C disbursement completes or fails
    /// </summary>
    /// <param name="callbackData">B2C result callback data</param>
    /// <returns>Callback response</returns>
    [HttpPost("b2c/result")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> B2CResultCallback([FromBody] B2CCallbackDto callbackData)
    {
        var validationResult = ValidateWebhookRequest();
        if (validationResult != null)
        {
            return validationResult;
        }

        var correlationId = GetCorrelationId();
        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });

        _logger.LogInformation("M-Pesa B2C result callback received: ConversationID={ConversationID}, ResultCode={ResultCode}",
            callbackData.Result?.ConversationID,
            callbackData.Result?.ResultCode);

        var result = await _mpesaService.HandleB2CCallbackAsync(callbackData);

        if (!result.IsSuccess)
        {
            _logger.LogError("Failed to process B2C callback: {Error}", result.ErrorMessage);
        }

        // Always return success to M-Pesa to acknowledge receipt
        return Ok(new
        {
            ResultCode = 0,
            ResultDesc = "Accepted"
        });
    }

    /// <summary>
    /// M-Pesa B2C timeout callback endpoint
    /// Called by Safaricom if B2C request times out
    /// </summary>
    /// <param name="callbackData">B2C timeout callback data</param>
    /// <returns>Callback response</returns>
    [HttpPost("b2c/timeout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult B2CTimeoutCallback([FromBody] object callbackData)
    {
        var validationResult = ValidateWebhookRequest();
        if (validationResult != null)
        {
            return validationResult;
        }

        var correlationId = GetCorrelationId();
        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });

        _logger.LogWarning("M-Pesa B2C timeout callback received: {CallbackData}",
            System.Text.Json.JsonSerializer.Serialize(callbackData));

        // Log the timeout for manual investigation
        // In production, you might want to mark the transaction as timed out

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

    private string GetCorrelationId()
    {
        if (Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId) &&
            !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return HttpContext.TraceIdentifier ?? Guid.NewGuid().ToString("N");
    }

    private IActionResult? ValidateWebhookRequest()
    {
        if (!IsAllowedIp())
        {
            _logger.LogWarning("M-Pesa webhook request blocked: IP not allowed");
            return Unauthorized(new { message = "Unauthorized" });
        }

        if (string.IsNullOrWhiteSpace(_mpesaConfiguration.WebhookToken))
        {
            if (_environment.IsDevelopment())
            {
                return null;
            }

            _logger.LogWarning("M-Pesa webhook request blocked: missing webhook token configuration");
            return Unauthorized(new { message = "Unauthorized" });
        }

        if (!Request.Headers.TryGetValue("X-MPesa-Token", out var tokenHeader))
        {
            _logger.LogWarning("M-Pesa webhook request blocked: missing token header");
            return Unauthorized(new { message = "Unauthorized" });
        }

        if (!string.Equals(tokenHeader.ToString(), _mpesaConfiguration.WebhookToken, StringComparison.Ordinal))
        {
            _logger.LogWarning("M-Pesa webhook request blocked: invalid token");
            return Unauthorized(new { message = "Unauthorized" });
        }

        return null;
    }

    private bool IsAllowedIp()
    {
        var allowedIps = _mpesaConfiguration.AllowedCallbackIPs;
        if (allowedIps == null || allowedIps.Count == 0)
        {
            return true;
        }

        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        if (remoteIp == null)
        {
            return false;
        }

        var remoteIpString = remoteIp.ToString();
        return allowedIps.Any(ip => string.Equals(ip, remoteIpString, StringComparison.OrdinalIgnoreCase));
    }
}
