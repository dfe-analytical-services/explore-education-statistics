#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IEmailTemplateService
    {
        public Either<ActionResult, Unit> SendInviteEmail(string email);

        public Either<ActionResult, Unit> SendPublicationRoleEmail(
            string email,
            Publication publication,
            PublicationRole role);

        public Either<ActionResult, Unit> SendReleaseRoleEmail(
            string email,
            Release release,
            ReleaseRole role);
    }
}
