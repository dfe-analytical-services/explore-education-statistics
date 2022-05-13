#nullable enable
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

/// <summary>
/// Class representing the result of invoking a Signal R hub method.
/// </summary>
/// <remarks>
/// This mostly acts to provide interop with <see cref="ActionResult"/>
/// to allow us to connect hubs to service methods, allowing us to
/// write our service code as we would normally.
/// </remarks>
public record HubResult
{
    public string Type = nameof(HubResult);

    /// <summary>
    /// Status code for the result. Should use
    /// the same semantics as a HTTP status code.
    /// </summary>
    public int Status { get; }

    /// <summary>
    /// An optional message can be provided
    /// e.g. in the event of an error.
    /// </summary>
    public string? Message { get; }

    public HubResult(int status, string? message = null)
    {
        Status = status;
        Message = message;
    }

    public HubResult(ActionResult result, string? message = null)
    {
        Status = GetResultCode(result);
        Message = message;
    }

    private static int GetResultCode(ActionResult result)
    {
        return result switch
        {
            ForbidResult => StatusCodes.Status403Forbidden,
            StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
            _ => StatusCodes.Status204NoContent
        };
    }
}

/// <summary>
/// Class representing the result of successfully invoking a SignalR
/// hub method, returning some data as part of the response.
/// </summary>
/// <typeparam name="TData">The data returned by the response</typeparam>
public record HubResult<TData> : HubResult where TData : class
{
    private const int DefaultStatusCode = StatusCodes.Status200OK;

    public TData? Data { get; }

    public HubResult(TData data, int status = DefaultStatusCode) : base(status)
    {
        Data = data;
    }

    public HubResult(
        ActionResult<TData> result,
        int status = DefaultStatusCode) : base(status)
    {
        Data = result.Value;
    }

    public HubResult(ActionResult result) : base(result)
    {
        Data = null!;
    }
}