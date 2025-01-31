#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public record PreReleaseUserViewModel(string Email);

    public class PreReleaseUserInvitePlan
    {
        public List<string> AlreadyInvited { get; } = new();

        public List<string> AlreadyAccepted { get; } = new();

        public List<string> Invitable { get; } = new();
    }

    public class PreReleaseUserInviteViewModel
    {
        [MinLength(1, ErrorMessage = "Must have at least one email.")]
        public List<string> Emails { get; set; } = new();
    }

    public class PreReleaseUserRemoveRequest
    {
        [EmailAddress] public string Email { get; set; } = string.Empty;
    }
}
