#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;

public static class ReleaseCreationUtils
{
    /// <summary>
    /// Builds a new <see cref="Release"/>/<see cref="ReleaseVersion"/> pair from a create request, adds it to the
    /// context, and inserts its <see cref="ReleaseSeriesItem"/> - the steps common to <c>ReleaseService.CreateRelease</c>
    /// and <c>ReleaseRolloverService.CreateReleaseRollover</c>. Callers still do their own content initialisation and
    /// their own final <c>SaveChangesAsync</c>/<c>GetRelease</c>.
    /// </summary>
    public static ReleaseVersion BuildReleaseVersion(
        ContentDbContext context,
        IGuidGenerator guidGenerator,
        IUserService userService,
        Publication publication,
        ReleaseCreateRequest releaseCreateRequest,
        Organisation[] publishingOrganisations,
        Guid? rolloverSourceReleaseId = null
    )
    {
        var release = new Release
        {
            PublicationId = releaseCreateRequest.PublicationId,
            Year = releaseCreateRequest.Year,
            TimePeriodCoverage = releaseCreateRequest.TimePeriodCoverage,
            Slug = releaseCreateRequest.Slug,
            Label = FormatReleaseLabel(releaseCreateRequest.Label),
            RolloverSourceReleaseId = rolloverSourceReleaseId,
        };

        var newReleaseVersion = new ReleaseVersion
        {
            Id = guidGenerator.NewGuid(),
            Release = release,
            Type = releaseCreateRequest.Type!.Value,
            ApprovalStatus = ReleaseApprovalStatus.Draft,
            PublicationId = release.PublicationId,
            PublishingOrganisations = [.. publishingOrganisations],
            CreatedById = userService.GetUserId(),
        };

        context.ReleaseVersions.Add(newReleaseVersion);

        publication.ReleaseSeries.Insert(0, new ReleaseSeriesItem { Id = Guid.NewGuid(), ReleaseId = release.Id });
        context.Publications.Update(publication);

        return newReleaseVersion;
    }
}
