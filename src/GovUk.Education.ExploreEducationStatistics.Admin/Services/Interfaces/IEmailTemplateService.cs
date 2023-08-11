#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IEmailTemplateService
    {
        Either<ActionResult, Unit> SendInviteEmail(
            string email,
            List<UserReleaseInvite> userReleaseInvites,
            List<UserPublicationInvite> userPublicationInvites);

        Either<ActionResult, Unit> SendPublicationRoleEmail(
            string email,
            Publication publication,
            PublicationRole role);

        Either<ActionResult, Unit> SendReleaseRoleEmail(
            string email,
            Release release,
            ReleaseRole role);

        Either<ActionResult, Unit> SendReleaseHigherReviewEmail(string email, Release release);

        Either<ActionResult, Unit> SendMethodologyHigherReviewEmail(
            string email,
            Guid methodologyVersionId,
            string methodologyTitle);
    }
}
