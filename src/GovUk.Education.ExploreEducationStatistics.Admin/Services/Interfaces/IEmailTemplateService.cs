using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IEmailTemplateService
    {
        public void SendInviteEmail(string email);

        public void SendPublicationRoleEmail(string email, Publication publication, PublicationRole role);

        public void SendReleaseRoleEmail(string email, Release release, ReleaseRole role);
    }
}
