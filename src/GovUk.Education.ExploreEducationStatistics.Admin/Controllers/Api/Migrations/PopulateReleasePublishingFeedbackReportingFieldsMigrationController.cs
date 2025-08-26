using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Migrations;

/// <summary>
/// Migration endpoint for EES-6370.  To be removed as part of EES-6416.
/// </summary>
[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class PopulateReleasePublishingFeedbackReportingFieldsMigrationController(
    ContentDbContext context) : ControllerBase
{
    public class MigrationResult
    {
        // ReSharper disable once NotAccessedField.Global
        public int Processed;
    }

    [HttpPut("bau/populate-release-publishing-feedback-reporting-fields")]
    public async Task<MigrationResult> PopulateFields(
        CancellationToken cancellationToken = default)
    {
        var feedbackRows = context
            .ReleasePublishingFeedback
            .Include(f => f.ReleaseVersion)
            .ThenInclude(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .ToList();

        feedbackRows.ForEach(feedback =>
        {
            feedback.ReleaseTitle = feedback.ReleaseVersion.Release.Title;
            feedback.PublicationTitle = feedback.ReleaseVersion.Release.Publication.Title;
        });

        await context.SaveChangesAsync(cancellationToken);

        return new MigrationResult { Processed = feedbackRows.Count };
    }
}
