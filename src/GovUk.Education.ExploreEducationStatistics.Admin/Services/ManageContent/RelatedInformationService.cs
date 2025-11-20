#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RelatedInformation.Dtos;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.RelatedInformation.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;

public class RelatedInformationService(ContentDbContext contentDbContext, IUserService userService)
    : IRelatedInformationService
{
    public Task<Either<ActionResult, List<RelatedInformationDto>>> GetRelatedInformation(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    ) =>
        contentDbContext
            .ReleaseVersions.AsNoTracking()
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken: cancellationToken)
            //.OnSuccess(rv => UserServiceExtensions.CheckCanViewReleaseVersion(userService, rv))
            //.OnSuccess(rv => userService.CheckCanViewReleaseVersion(rv))
            .OnSuccess(rv => rv.RelatedInformation.Select(RelatedInformationDto.FromModel).ToList());

    public Task<Either<ActionResult, List<RelatedInformationDto>>> CreateRelatedInformation(
        Guid releaseVersionId,
        RelatedInformationCreateRequest request,
        CancellationToken cancellationToken = default
    ) =>
        contentDbContext
            .ReleaseVersions.CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                if (releaseVersion.RelatedInformation == null)
                {
                    releaseVersion.RelatedInformation = new List<Link>();
                }

                releaseVersion.RelatedInformation.Add(
                    new Link
                    {
                        Id = Guid.NewGuid(),
                        Description = request.Title,
                        Url = request.Url,
                    }
                );

                contentDbContext.ReleaseVersions.Update(releaseVersion);
                await contentDbContext.SaveChangesAsync(cancellationToken);
                return releaseVersion.RelatedInformation;
            });

    public Task<Either<ActionResult, List<RelatedInformationDto>>> UpdateRelatedInformation(
        Guid releaseVersionId,
        List<RelatedInformationCreateRequest> updatedLinkRequests,
        CancellationToken cancellationToken = default
    ) =>
        persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var updatedLinks = new List<Link>();

                updatedLinkRequests.ForEach(r =>
                    updatedLinks.Add(
                        new Link
                        {
                            Id = Guid.NewGuid(),
                            Description = r.Title,
                            Url = r.Url,
                        }
                    )
                );

                releaseVersion.RelatedInformation = updatedLinks;
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return releaseVersion.RelatedInformation;
            });

    public Task<Either<ActionResult, List<RelatedInformationDto>>> DeleteRelatedInformation(
        Guid releaseVersionId,
        Guid relatedInformationId,
        CancellationToken cancellationToken = default
    ) =>
        persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                releaseVersion.RelatedInformation.Remove(
                    releaseVersion.RelatedInformation.Find(item => item.Id == relatedInformationId)
                );

                contentDbContext.ReleaseVersions.Update(releaseVersion);
                await contentDbContext.SaveChangesAsync(cancellationToken);
                return releaseVersion.RelatedInformation;
            });
}
