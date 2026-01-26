namespace RentCollection.Application.DTOs.Payments;

/// <summary>
/// M-Pesa B2C result callback DTO
/// </summary>
public class B2CCallbackDto
{
    public ResultDto? Result { get; set; }
}

public class ResultDto
{
    public int ResultType { get; set; }
    public int ResultCode { get; set; }
    public string ResultDesc { get; set; } = string.Empty;
    public string OriginatorConversationID { get; set; } = string.Empty;
    public string ConversationID { get; set; } = string.Empty;
    public string TransactionID { get; set; } = string.Empty;
    public ResultParametersDto? ResultParameters { get; set; }
    public ReferenceDataDto? ReferenceData { get; set; }
}

public class ResultParametersDto
{
    public List<ResultParameterDto> ResultParameter { get; set; } = new();
}

public class ResultParameterDto
{
    public string Key { get; set; } = string.Empty;
    public object? Value { get; set; }
}

public class ReferenceDataDto
{
    public ReferenceItemDto? ReferenceItem { get; set; }
}

public class ReferenceItemDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
