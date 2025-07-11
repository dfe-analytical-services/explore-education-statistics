using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;

[Route("api")]
[ApiController]
public class AnalyticsController(
    IAnalyticsManager analyticsManager) 
    : ControllerBase
{
    /// <summary>
    /// A user can download the data in the table builder tool.
    /// This action is implemented entirely in the front end.
    /// Therefore this end point exists for the front end to
    /// invoke to record the user action and the corresponding
    /// query parameters on the page.
    /// </summary>
    /// <param name="requestBindingModel">The currently set filter and query values in the table builder tool.</param>
    /// <param name="cancellationToken">a cancellation token</param>
    /// <returns>OK, unless the request payload is invalid, in which case a Bad Request with the error details</returns>
    [HttpPost("tablebuilder/analytics")]
    public async Task<ActionResult> RecordDownload(
        [FromBody] RecordTableToolDownloadRequestBindingModel requestBindingModel,
        CancellationToken cancellationToken = default)
    {
        var callCapture = requestBindingModel.ToModel();
        await analyticsManager.Add(callCapture, cancellationToken);
        return new AcceptedResult();
    }
    
    /// <summary>
    /// A user can download the data in the table on a permaLink page.
    /// This action is implemented entirely in the front end.
    /// Therefore this end point exists for the front end to
    /// invoke to record the user action and the corresponding
    /// information on the page.
    /// A permaLink page is a static snapshot of a query result content.
    /// It no longer contains the query settings or ids hence
    /// the much reduced set of information captured.
    /// </summary>
    /// <param name="requestBindingModel">The few values associated with this page, such as the permaLink id.</param>
    /// <param name="cancellationToken">a cancellation token</param>
    /// <returns>OK, unless the request payload is invalid, in which case a Bad Request with the error details</returns>
    [HttpPost("permalink/analytics")]
    public async Task<ActionResult> RecordDownload(
        [FromBody] RecordPermalinkTableDownloadRequestBindingModel requestBindingModel,
        CancellationToken cancellationToken = default)
    {
        var callCapture = requestBindingModel.ToModel();
        await analyticsManager.Add(callCapture, cancellationToken);
        return new AcceptedResult();
    }
}
