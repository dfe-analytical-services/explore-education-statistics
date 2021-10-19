#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class PreReleaseUserViewModel
    {
        public PreReleaseUserViewModel(string email)
        {
            Email = email;
        }

        public string Email { get; }
    }

    public class PreReleaseUserInvitePlan
    {
        public List<string> AlreadyInvited { get; } = new List<string>();

        public List<string> AlreadyAccepted { get; } = new List<string>();

        public List<string> Invitable { get; } = new List<string>();
    }

    public class PreReleaseUserInviteRequest
    {
        [Required] public List<string> Emails { get; set; } = new List<string>();
    }

    public class PreReleaseUserRemoveRequest
    {
        [Required] public string Email { get; set; } = string.Empty;
    }
}
