using RentCollection.Application.Common.Models;

namespace RentCollection.Application.Common;

/// <summary>
/// Type alias for Result - used in service layer
/// </summary>
public class ServiceResult : Result
{
}

/// <summary>
/// Type alias for Result<T> - used in service layer
/// </summary>
public class ServiceResult<T> : Result<T>
{
}
