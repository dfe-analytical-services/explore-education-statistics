#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;

public class RelatedInformationService(
    ContentDbContext context,
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IUserService userService) : IRelatedInformationService
{
    public Task<Either<ActionResult, List<Link>>> GetRelatedInformationAsync(Guid releaseVersionId)
    {
        return persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(releaseVersion => releaseVersion.RelatedInformation);
    }

    public Task<Either<ActionResult, List<Link>>> AddRelatedInformationAsync(Guid releaseVersionId,
        CreateUpdateLinkRequest request)
    {
        return persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                if (releaseVersion.RelatedInformation == null)
                {
                    releaseVersion.RelatedInformation = new List<Link>();
                }

                releaseVersion.RelatedInformation.Add(new Link
                {
                    Id = Guid.NewGuid(),
                    Description = request.Description,
                    Url = request.Url
                });

                context.ReleaseVersions.Update(releaseVersion);
                await context.SaveChangesAsync();
                return releaseVersion.RelatedInformation;
            });
    }

    public Task<Either<ActionResult, List<Link>>> UpdateRelatedInformation(
        Guid releaseVersionId,
        List<CreateUpdateLinkRequest> updatedLinkRequests,
        CancellationToken cancellationToken = default)
    {
        return persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var updatedLinks = new List<Link>();

                updatedLinkRequests.ForEach(r => updatedLinks.Add(
                    new Link
                    {
                        Id = Guid.NewGuid(),
                        Description = r.Description,
                        Url = r.Url
                    }));

                releaseVersion.RelatedInformation = updatedLinks;
                await context.SaveChangesAsync(cancellationToken);

                return releaseVersion.RelatedInformation;
            });
    }

    public Task<Either<ActionResult, List<Link>>> DeleteRelatedInformationAsync(Guid releaseVersionId,
        Guid relatedInformationId)
    {
        return persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                releaseVersion.RelatedInformation.Remove(
                    releaseVersion.RelatedInformation.Find(item => item.Id == relatedInformationId));

                context.ReleaseVersions.Update(releaseVersion);
                await context.SaveChangesAsync();
                return releaseVersion.RelatedInformation;
            });
    }
}
