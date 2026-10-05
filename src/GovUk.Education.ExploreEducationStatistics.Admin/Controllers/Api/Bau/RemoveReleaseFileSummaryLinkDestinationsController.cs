#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Repositories;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

/// <summary>
/// Removes only link destinations introduced by the HTML summary migration. Original HTML is required:
/// parenthesized URLs in plain text alone cannot establish whether a destination was generated.
/// </summary>
[Route("api/bau")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class RemoveReleaseFileSummaryLinkDestinationsController(
    ContentDbContext context,
    IReleaseFileSummaryLinkCleanupRepository repository
) : ControllerBase
{
    // TODO EES-7367 Remove once run in all environments.
    [HttpGet("remove-release-file-summary-link-destinations")]
    public Task<ActionResult<SummaryLinkCleanupReportViewModel>> DryRun(
        [FromQuery] int limit = 500,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default
    ) => BuildReport(false, limit, offset, cancellationToken);

    /// <summary>
    /// Rebuilds and applies this page of the report; each replacement is independently atomic.
    /// Follow NextOffset to inspect every row, including skipped rows. Repeating a page is safe.
    /// A failure can leave earlier rows committed; retrying the page will report them as already cleaned.
    /// </summary>
    [HttpPut("remove-release-file-summary-link-destinations")]
    public Task<ActionResult<SummaryLinkCleanupReportViewModel>> Apply(
        [FromQuery] int limit = 500,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default
    ) => BuildReport(true, limit, offset, cancellationToken);

    private async Task<ActionResult<SummaryLinkCleanupReportViewModel>> BuildReport(
        bool apply,
        int limit,
        int offset,
        CancellationToken cancellationToken
    )
    {
        if (limit is < 1 or > 500 || offset < 0)
        {
            return BadRequest("Limit must be between 1 and 500 and offset must be non-negative.");
        }

        // Do not filter by link text: cleaned and skipped rows must remain in the same pagination sequence.
        var query = context.ReleaseFiles.AsNoTracking().Where(file => file.Summary != null);
        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(file => file.Id)
            .Skip(offset)
            .Take(limit)
            .Select(file => new { file.Id, file.Summary })
            .ToListAsync(cancellationToken);
        var backup = await repository.GetOriginalSummaries(rows.Select(row => row.Id).ToList(), cancellationToken);
        var report = new SummaryLinkCleanupReportViewModel
        {
            SourceBackupExists = backup.Exists,
            TotalSummaryCount = total,
            InspectedCount = rows.Count,
            NextOffset = (long)offset + rows.Count < total ? offset + rows.Count : null,
        };

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!backup.Exists)
            {
                report.Skipped.Add(new(row.Id, "MissingSourceBackup"));
                continue;
            }
            if (!backup.Summaries.TryGetValue(row.Id, out var html) || html == null)
            {
                report.Skipped.Add(new(row.Id, "MissingSourceHtml"));
                continue;
            }

            var converted = HtmlToTextUtils.HtmlToText(html);
            var cleaned = HtmlToTextUtils.HtmlToText(html, includeLinkDestinations: false);
            if (string.Equals(converted, cleaned, StringComparison.Ordinal))
            {
                report.Skipped.Add(new(row.Id, "NoLinkDestinations"));
            }
            else if (string.Equals(row.Summary, cleaned, StringComparison.Ordinal))
            {
                report.Skipped.Add(new(row.Id, "AlreadyCleaned"));
            }
            else if (!string.Equals(row.Summary, converted, StringComparison.Ordinal))
            {
                // Includes summaries still containing HTML, or edited after the original migration backup.
                report.Skipped.Add(new(row.Id, "CurrentSummaryDoesNotMatchSourceConversion"));
            }
            else if (apply && !await repository.TryApply(row.Id, row.Summary!, cleaned, cancellationToken))
            {
                report.Skipped.Add(new(row.Id, "ConcurrentChangeOrExistingCleanupBackup"));
            }
            else
            {
                report.Changes.Add(new(row.Id, row.Summary!, cleaned));
                if (apply)
                    report.AppliedCount++;
            }
        }
        return report;
    }
}

public record SummaryLinkCleanupReportViewModel
{
    public bool SourceBackupExists { get; init; }
    public int TotalSummaryCount { get; init; }
    public int InspectedCount { get; init; }
    public int? NextOffset { get; init; }
    public List<SummaryLinkCleanupChangeViewModel> Changes { get; } = [];
    public List<SummaryLinkCleanupSkipViewModel> Skipped { get; } = [];
    public int ChangeCount => Changes.Count;
    public int SkippedCount => Skipped.Count;
    public int AppliedCount { get; set; }
}

public record SummaryLinkCleanupChangeViewModel(Guid ReleaseFileId, string Current, string PlainText);

public record SummaryLinkCleanupSkipViewModel(Guid ReleaseFileId, string Reason);
