#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IEmailTemplateService
{
    Either<ActionResult, Unit> SendInviteEmail(
        string email,
        HashSet<(string PublicationTitle, string ReleaseTitle, ReleaseRole Role)> releaseRolesInfo,
        HashSet<(string PublicationTitle, PublicationRole Role)> publicationRolesInfo
    );

    Either<ActionResult, Unit> SendPublicationRoleEmail(string email, string publicationTitle, PublicationRole role);

    Either<ActionResult, Unit> SendReleaseRoleEmail(
        string email,
        string publicationTitle,
        string releaseTitle,
        Guid publicationId,
        Guid releaseVersionId,
        ReleaseRole role
    );

    Either<ActionResult, Unit> SendContributorInviteEmail(
        string email,
        string publicationTitle,
        HashSet<(int Year, TimeIdentifier TimePeriodCoverage, string Title)> releasesInfo
    );

    Either<ActionResult, Unit> SendPreReleaseInviteEmail(
        string email,
        string publicationTitle,
        string releaseTitle,
        bool isNewUser,
        Guid publicationId,
        Guid releaseVersionId,
        DateTimeOffset preReleaseWindowStart,
        DateTimeOffset publishScheduled
    );

    Either<ActionResult, Unit> SendReleaseHigherReviewEmail(string email, ReleaseVersion releaseVersion);

    Either<ActionResult, Unit> SendMethodologyHigherReviewEmail(
        string email,
        Guid methodologyVersionId,
        string methodologyTitle
    );
}
