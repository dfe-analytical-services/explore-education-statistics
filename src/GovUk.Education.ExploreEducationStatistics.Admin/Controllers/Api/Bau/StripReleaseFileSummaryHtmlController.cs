#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

/// <summary>
/// Converts any <see cref="ReleaseFile.Summary"/> still containing HTML to plain text.
///
/// EES-4353 changed the data guidance content field from a rich text field to a plain text one, but did not
/// migrate the summaries created before it. Until they have been converted, this field can hold either.
/// </summary>
[Route("api/bau")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class StripReleaseFileSummaryHtmlController(ContentDbContext contentDbContext, IRawSqlExecutor sqlExecutor)
    : ControllerBase
{
    private const string BackupTable = "ReleaseFiles_Ees7367_SummaryBackup";

    private const string BackupSql = $"""
        IF OBJECT_ID('dbo.{BackupTable}', 'U') IS NULL
            SELECT Id, Summary
            INTO dbo.{BackupTable}
            FROM dbo.ReleaseFiles
            WHERE Summary IS NOT NULL;
        """;

    /// <summary>
    /// Reports the changes that <see cref="StripSummaryHtml"/> would make, without making them.
    /// </summary>
    // TODO EES-7367 Remove once run in all environments
    [HttpGet("strip-release-file-summary-html")]
    public async Task<ActionResult<StripSummaryHtmlReportViewModel>> DryRunStripSummaryHtml(
        [FromQuery] int limit = 500,
        CancellationToken cancellationToken = default
    ) => await BuildReport(limit, cancellationToken);

    /// <summary>
    /// Converts the summaries reported by <see cref="DryRunStripSummaryHtml"/> to plain text, backing up all
    /// existing summaries beforehand.
    ///
    /// Only <paramref name="limit"/> summaries are converted per request, to avoid exceeding the request
    /// timeout. Converted summaries no longer match the query, so call this repeatedly until the reported
    /// <see cref="StripSummaryHtmlReportViewModel.CandidateCount"/> is zero.
    /// </summary>
    // TODO EES-7367 Remove once run in all environments
    [HttpPut("strip-release-file-summary-html")]
    public async Task<ActionResult<StripSummaryHtmlReportViewModel>> StripSummaryHtml(
        [FromQuery] int limit = 500,
        CancellationToken cancellationToken = default
    )
    {
        // Back up every existing summary before changing any of them. This is a no-op once the backup exists,
        // so the backup is always of the summaries as they were before the first of these requests.
        await sqlExecutor.ExecuteSqlRaw(contentDbContext, BackupSql, cancellationToken);

        var report = await BuildReport(limit, cancellationToken);

        var plainTextByReleaseFileId = report.Changes.ToDictionary(
            change => change.ReleaseFileId,
            change => change.PlainText
        );

        var releaseFiles = await contentDbContext
            .ReleaseFiles.Where(rf => plainTextByReleaseFileId.Keys.Contains(rf.Id))
            .ToListAsync(cancellationToken);

        releaseFiles.ForEach(releaseFile => releaseFile.Summary = plainTextByReleaseFileId[releaseFile.Id]);

        await contentDbContext.SaveChangesAsync(cancellationToken);

        return report;
    }

    private async Task<StripSummaryHtmlReportViewModel> BuildReport(int limit, CancellationToken cancellationToken)
    {
        var candidates = contentDbContext
            .ReleaseFiles.AsNoTracking()
            .Where(rf =>
                rf.Summary != null
                // Summaries containing markup or a character entity may not be plain text. Anything else
                // cannot be HTML, so is left alone.
                && (
                    (rf.Summary.Contains("<") && rf.Summary.Contains(">"))
                    || (rf.Summary.Contains("&") && rf.Summary.Contains(";"))
                )
            );

        var releaseFiles = await candidates
            .OrderBy(rf => rf.Id)
            .Select(rf => new { rf.Id, rf.Summary })
            .Take(limit)
            .ToListAsync(cancellationToken);

        var changes = releaseFiles
            .Select(rf => new SummaryChangeViewModel
            {
                ReleaseFileId = rf.Id,
                Current = rf.Summary!,
                PlainText = HtmlToTextUtils.HtmlToText(rf.Summary!),
            })
            // A character entity match is only a candidate. Discard the ones that are already plain text.
            .Where(change => change.PlainText != change.Current)
            .ToList();

        return new StripSummaryHtmlReportViewModel
        {
            CandidateCount = await candidates.CountAsync(cancellationToken),
            Changes = changes,
        };
    }
}

public record StripSummaryHtmlReportViewModel
{
    /// <summary>
    /// The total number of summaries matching the query, which may exceed the number inspected.
    /// </summary>
    public required int CandidateCount { get; init; }

    public required List<SummaryChangeViewModel> Changes { get; init; }

    public int ChangeCount => Changes.Count;
}

public record SummaryChangeViewModel
{
    public required Guid ReleaseFileId { get; init; }
    public required string Current { get; init; }
    public required string PlainText { get; init; }
}
