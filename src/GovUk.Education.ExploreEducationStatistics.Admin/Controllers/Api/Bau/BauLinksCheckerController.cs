using System.Globalization;
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.LinkChecker;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.LinkChecker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api/bau/links/check")]
[ApiController]
[Authorize]
public class BauLinksCheckerController(ILinkCheckerQueue queue) : ControllerBase
{
    [HttpPost]
    public IActionResult StartLinkCheck()
    {
        var jobId = queue.EnqueueJob();
        var location = Url.Action(nameof(GetLinkCheckStatus), new { jobId }) ?? $"/api/bau/links/check/{jobId}";
        return Accepted(location, new StartLinkResponse(jobId, location));
    }

    [HttpGet("{jobId:guid}")]
    public IActionResult GetLinkCheckStatus(Guid jobId)
    {
        if (!queue.TryGetJob(jobId, out var job))
        {
            return NotFound();
        }

        var result = job!;

        return Ok(
            new LinkCheckerJob
            {
                Id = result.Id,
                Status = result.Status,
                CreatedAt = result.CreatedAt,
                StartedAt = result.StartedAt,
                CompletedAt = result.CompletedAt,
                ErrorMessage = result.ErrorMessage,
                Results = result.Status == LinkCheckerJobStatus.Completed ? result.Results : null,
            }
        );
    }

    [HttpPost("{jobId:guid}/cancel")]
    public IActionResult CancelLinkCheck(Guid jobId)
    {
        if (!queue.TryGetJob(jobId, out var job))
        {
            return NotFound();
        }

        if (
            job!.Status
            is LinkCheckerJobStatus.Completed
                or LinkCheckerJobStatus.Failed
                or LinkCheckerJobStatus.Canceled
        )
        {
            return BadRequest(
                new
                {
                    job.Id,
                    job.Status,
                    message = "Job cannot be canceled.",
                }
            );
        }

        if (!queue.TryCancelJob(jobId))
        {
            return Conflict(
                new
                {
                    job.Id,
                    job.Status,
                    message = "Cancel request failed.",
                }
            );
        }

        return Accepted(
            new
            {
                job.Id,
                job.Status,
                message = "Cancellation requested.",
            }
        );
    }

    [HttpGet("{jobId:guid}/results")]
    public IActionResult GetLinkCheckResults(Guid jobId)
    {
        if (!queue.TryGetJob(jobId, out var job))
        {
            return NotFound();
        }

        var result = job!;
        return result.Status switch
        {
            LinkCheckerJobStatus.Completed => Ok(result.Results),
            LinkCheckerJobStatus.Canceled => StatusCode(410, new { result.Status, result.ErrorMessage }),
            LinkCheckerJobStatus.Failed => StatusCode(500, new { result.Status, result.ErrorMessage }),
            _ => Accepted(
                new
                {
                    result.Id,
                    result.Status,
                    statusUrl = Url.Action(nameof(GetLinkCheckStatus), new { jobId }) ?? $"/api/links/check/{jobId}",
                }
            ),
        };
    }

    [HttpGet("{jobId:guid}/download-csv")]
    public IActionResult DownloadCsv(Guid jobId)
    {
        if (!queue.TryGetJob(jobId, out var job))
        {
            return NotFound();
        }

        var result = job!;

        return result.Status switch
        {
            LinkCheckerJobStatus.Completed => Download(result.Results),
            LinkCheckerJobStatus.Canceled => StatusCode(410, new { result.Status, result.ErrorMessage }),
            LinkCheckerJobStatus.Failed => StatusCode(500, new { result.Status, result.ErrorMessage }),
            _ => Accepted(
                new
                {
                    result.Id,
                    result.Status,
                    statusUrl = Url.Action(nameof(GetLinkCheckStatus), new { jobId }) ?? $"/api/links/check/{jobId}",
                }
            ),
        };

        IActionResult Download(List<LinksCsvItem> results)
        {
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(result.Results);
                writer.Flush();

                return File(memoryStream.ToArray(), "text/csv", "export.csv");
            }
        }
    }
}
