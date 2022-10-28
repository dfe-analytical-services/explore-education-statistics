#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IEmailTemplateService
    {
        public Either<ActionResult, Unit> SendInviteEmail(
            string email,
            List<UserReleaseInvite> userReleaseInvites,
            List<UserPublicationInvite> userPublicationInvites);

        public Either<ActionResult, Unit> SendPublicationRoleEmail(
            string email,
            Publication publication,
            PublicationRole role);

        public Either<ActionResult, Unit> SendReleaseRoleEmail(
            string email,
            Release release,
            ReleaseRole role);

        public Either<ActionResult, Unit> SendReleaseApproverEmail(
            string email,
            Release release);
    }
}
