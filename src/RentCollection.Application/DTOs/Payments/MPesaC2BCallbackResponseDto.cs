namespace RentCollection.Application.DTOs.Payments
{
    /// <summary>
    /// Response DTO for M-Pesa C2B callback endpoints (validation and confirmation)
    /// </summary>
    public class MPesaC2BCallbackResponseDto
    {
        /// <summary>
        /// Result code - 0 means success/accepted
        /// </summary>
        public int ResultCode { get; set; }

        /// <summary>
        /// Result description message
        /// </summary>
        public string ResultDesc { get; set; } = string.Empty;
    }
}
