#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleasePublishingStatusService(
    ContentDbContext contentDbContext,
    IPublisherTableStorageService publisherTableStorageService,
    IUserService userService
) : IReleasePublishingStatusService
{
    public async Task<Either<ActionResult, ReleasePublishingStatusViewModel>> GetReleaseStatus(Guid releaseVersionId) =>
        await contentDbContext
            .ReleaseVersions.AsNoTracking()
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var asyncPageable = await publisherTableStorageService.QueryEntities<ReleasePublishingStatus>(
                    PublisherReleaseStatusTableName,
                    status => status.PartitionKey == releaseVersion.Id.ToString()
                );
                var statusList = await asyncPageable.ToListAsync();
                var latestStatus = statusList.MaxBy(status => status.Created);

                return ReleasePublishingStatusViewModel.FromReleasePublishingStatus(latestStatus);
            });
}
