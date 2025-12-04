using RentCollection.Application.Common.Models;

namespace RentCollection.Application.Common;

/// <summary>
/// Type alias for Result - used in service layer
/// </summary>
public class ServiceResult : Result
{
    public new static ServiceResult Success(string? message = null) => new()
    {
        IsSuccess = true,
        Message = message
    };

    public new static ServiceResult Failure(string error) => new()
    {
        IsSuccess = false,
        Errors = new List<string> { error }
    };

    public new static ServiceResult Failure(List<string> errors) => new()
    {
        IsSuccess = false,
        Errors = errors
    };
}

/// <summary>
/// Type alias for Result<T> - used in service layer
/// </summary>
public class ServiceResult<T> : Result<T>
{
    public new static ServiceResult<T> Success(T data, string? message = null) => new()
    {
        IsSuccess = true,
        Data = data,
        Message = message
    };

    public new static ServiceResult<T> Failure(string error) => new()
    {
        IsSuccess = false,
        Errors = new List<string> { error }
    };

    public new static ServiceResult<T> Failure(List<string> errors) => new()
    {
        IsSuccess = false,
        Errors = errors
    };
}
