#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IEmailTemplateService
{
    Task<Either<ActionResult, Unit>> SendInviteEmail(
        string email,
        HashSet<Guid> userReleaseRoleIds,
        HashSet<Guid> userPublicationRoleIds
    );

    Either<ActionResult, Unit> SendPublicationRoleEmail(string email, string publicationTitle, PublicationRole role);

    Either<ActionResult, Unit> SendReleaseRoleEmail(string email, ReleaseVersion releaseVersion, ReleaseRole role);

    Task<Either<ActionResult, Unit>> SendContributorInviteEmail(
        string email,
        string publicationTitle,
        HashSet<Guid> releaseVersionIds
    );

    Task<Either<ActionResult, Unit>> SendPreReleaseInviteEmail(string email, Guid releaseVersionId, bool isNewUser);

    Either<ActionResult, Unit> SendReleaseHigherReviewEmail(string email, ReleaseVersion releaseVersion);

    Either<ActionResult, Unit> SendMethodologyHigherReviewEmail(
        string email,
        Guid methodologyVersionId,
        string methodologyTitle
    );
}
