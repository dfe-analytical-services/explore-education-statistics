#nullable enable
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ContributorInviteViewModel
    {
        [EmailAddress] public string Email { get; set; } = string.Empty;
    }
}
